using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace PolygonEditor.Model
{
    public class MyPolygon
    {
        public MyPolygon(Canvas canvas)
        {
            vertices = new List<Vertice>();
            edges = new List<Edge>();
            this.canvas = canvas;
            Completed = false;
            InitializeContextMenus();
        }

        public Vertice LeftClickedVertice = null;
        public Vertice FirstVertice => VerticesCount > 0 ? vertices[0] : null;
        public bool Completed
        {
            get { return _completed; }
            set { _completed = value; }
        }

        private List<Vertice> vertices;
        private int VerticesCount { get => vertices.Count; }
        private Vertice LastVertice => VerticesCount > 0 ? vertices[VerticesCount - 1] : null;
        private Vertice RightClickedVertice = null;

        private List<Edge> edges;
        private int EdgesCount { get => edges.Count; }
        private Edge RightClickedEdge = null;

        private bool _completed;

        private List<Point> snapShotPoints;
        private Canvas canvas;
        private ContextMenu verticeMenu;
        private ContextMenu edgeMenu;

        private void InitializeContextMenus()
        {
            InitalizeVerticeContextMenu();
            InitalizeEdgeContextMenu();
        }
        private void InitalizeVerticeContextMenu()
        {
            verticeMenu = new ContextMenu();
            MenuItem delete = new MenuItem();
            delete.Header = "Delete vertice";
            delete.Click += DeleteVertice;
            verticeMenu.Items.Add(delete);
        }
        private void InitalizeEdgeContextMenu()
        {
            edgeMenu = new ContextMenu();

            MenuItem horizontalConstraint = new MenuItem();
            horizontalConstraint.Header = "Add horizontal constraint";
            horizontalConstraint.Click += AddHorizontalConstraint;

            MenuItem verticalConstraint = new MenuItem();
            verticalConstraint.Header = "Add vertical constraint";
            verticalConstraint.Click += AddVerticalConstraint;

            MenuItem lenghtConstraint = new MenuItem();
            lenghtConstraint.Header = "Add lenght constraint";
            lenghtConstraint.Click += AddLenghtConstraint;

            edgeMenu.Items.Add(horizontalConstraint);
            edgeMenu.Items.Add(verticalConstraint);
            edgeMenu.Items.Add(lenghtConstraint);
        }


        #region Polygon_Modification

        public string Serialize()
        {
            string result = "";
            foreach (Vertice v in vertices)
                result += ((int)v.point.X).ToString() + "." + ((int)v.point.Y).ToString() + "|";
            result += Environment.NewLine;
            foreach(Edge e in edges)
            {
                string constraintSymbol = "";
                if (!e.HasConstraint)
                    constraintSymbol = "N|";
                else
                {
                    if (e.constraint.GetType() == typeof(VerticalConstraint))
                        constraintSymbol = "V|";
                    if (e.constraint.GetType() == typeof(HorizontalConstraint))
                        constraintSymbol = "H|";
                    if (e.constraint.GetType() == typeof(LengthConstraint))
                        constraintSymbol = "L" + ((int)e.Length).ToString() + "|";
                }

                result += constraintSymbol;
            }

            return result;
        }

        public void Deserialize(string polygon)
        {
            var result = polygon.Split('\n');
            string allPoints = result[0];
            string allConstraints = result[1];
            var points = allPoints.Split('|');
            points = points.Take(points.Count() - 1).ToArray();
            var constraints = allConstraints.Split('|');
            constraints = constraints.Take(constraints.Count() - 1).ToArray();

            foreach (var point in points)
            {
                var p = point.Split('.');
                AddVertice(new Point(Int32.Parse(p[0]), Int32.Parse(p[1])));
            }
            edges.Add(new Edge(vertices[vertices.Count - 2], vertices[vertices.Count - 1], null, canvas));
            for(int i=0; i <constraints.Length; i++)
            {
                var constr = constraints[i];
                if(constr[0] == 'V')
                {
                    edges[i].constraint = new VerticalConstraint(edges[i]);
                }
                if (constr[0] == 'H')
                {
                    edges[i].constraint = new HorizontalConstraint(edges[i]);
                }
                if (constr[0] == 'L')
                {
                    var clength = constr.Substring(1, constr.Length - 1);
                    edges[i].constraint = new LengthConstraint(edges[i], Int32.Parse(clength) );
                }
            }

            Completed = true;
            Complete();


        }

        public void DrawPreviewLine(Point mouseCords)
        {
            ReDraw();
            Path previeLine = new Path();
            GeometryGroup gg = new GeometryGroup();
            previeLine.Fill = Brushes.Blue;
            Helpers.BresenhamLine((int)LastVertice.point.X, (int)LastVertice.point.Y,
                                    (int)mouseCords.X, (int)mouseCords.Y, gg);
            previeLine.Data = gg;
            previeLine.IsHitTestVisible = false;
            canvas.Children.Add(previeLine);
        }
        public void Move(int dx, int dy)
        {
            foreach (var v in vertices)
                v.point = new Point(v.point.X + dx, v.point.Y + dy);
            ReDraw();
        }
        public Rect GetBoundingBox()
        {
            double minX = vertices[0].point.X, minY = vertices[0].point.Y;
            double maxX = vertices[0].point.X, maxY = vertices[0].point.Y;
            foreach (var v in vertices)
            {
                if (v.point.X > maxX)
                    maxX = v.point.X;
                if (v.point.X < minX)
                    minX = v.point.X;
                if (v.point.Y > maxY)
                    maxY = v.point.Y;
                if (v.point.Y < minY)
                    minY = v.point.Y;
            }
            return new Rect(new Point(minX, minY), new Point(maxX, maxY));
        }

        public void ReDraw()
        {
            foreach (var e in edges)
                e.Draw();
            foreach (var v in vertices)
                v.Draw();
        }

        private void Complete()
        {
            foreach (var vertice in vertices)
                vertice.color = Brushes.Green;

            Completed = true;
            Edge newEdge = new Edge(LastVertice, FirstVertice, OnEdgeClicked, canvas);
            edges.Add(newEdge);
            LeftClickedVertice = null;
            ReDraw();
        }
        private void FailInformation(string info)
        {
            MessageBox.Show(info, "Error");
        }
        private void MakeSnapshot()
        {
            snapShotPoints = vertices.Select(v => new Point(v.point.X, v.point.Y)).ToList();
        }
        private void UseSnapshot()
        {
            for (int i = 0; i < VerticesCount; i++)
                vertices[i].point = new Point(snapShotPoints[i].X, snapShotPoints[i].Y);
        }
        #endregion

        #region Vertice_Modification
        public void MoveVertice(Point newPosition)
        {
            MakeSnapshot();
            LeftClickedVertice.point = newPosition;

            ReDraw();
            RepairConstraintsWhileMoving();
            ReDraw();
        }
        public void AddVertice(Point p)
        {
            Vertice newVertice = new Vertice(p, OnVerticeClicked, canvas);
            vertices.Add(newVertice);
            newVertice.Draw();
            if (VerticesCount > 1)
            {
                Edge newEdge = new Edge(vertices[VerticesCount - 2], LastVertice, OnEdgeClicked, canvas);
                newEdge.Draw();
                edges.Add(newEdge);
            }
        }
        public void DeleteVertice(object sender, RoutedEventArgs e)
        {
            if (VerticesCount > 3)
            {
                int delPosition = vertices.FindIndex((v) => v == RightClickedVertice);

                if (delPosition == 0)
                {
                    edges[VerticesCount - 1].v2 = vertices[delPosition + 1];
                    RemoveConstraints(edges[VerticesCount - 1]);
                }
                else if (delPosition == VerticesCount - 1)
                {
                    edges[delPosition - 1].v2 = vertices[0];
                    RemoveConstraints(edges[delPosition - 1]);
                }
                else
                {
                    edges[delPosition - 1].v2 = vertices[delPosition + 1];
                    RemoveConstraints(edges[delPosition - 1]);
                }

                vertices.RemoveAt(delPosition);
                RightClickedVertice = null;
                edges.RemoveAt(delPosition);
                ReDraw();
            }
            else
                FailInformation("Nie można usunąć wierzchołka");

        }
        public void AddMidVertice(Edge clickedEdge)
        {
            Point midPoint = new Point((clickedEdge.v1.point.X + clickedEdge.v2.point.X) / 2, (clickedEdge.v1.point.Y + clickedEdge.v2.point.Y) / 2);
            Vertice midVertice = new Vertice(midPoint, OnVerticeClicked, canvas);
            midVertice.color = Brushes.Green;

            int verticeInsertPosition = vertices.FindIndex(v => v == clickedEdge.v2);
            if (verticeInsertPosition == 0)
                vertices.Add(midVertice);
            else
                vertices.Insert(verticeInsertPosition, midVertice);

            Edge e1 = new Edge(clickedEdge.v1, midVertice, OnEdgeClicked, canvas);
            Edge e2 = new Edge(midVertice, clickedEdge.v2, OnEdgeClicked, canvas);
            int edgeInsertPosition = edges.FindIndex(edg => edg == clickedEdge);
            edges.Insert(edgeInsertPosition, e2);
            edges.Insert(edgeInsertPosition, e1);
            edges.Remove(clickedEdge);

            ReDraw();
        }

        private void OnVerticeClicked(object sender, MouseEventArgs e)
        {
            Vertice v = sender as Vertice;

            if (e.RightButton == MouseButtonState.Pressed)
            {
                verticeMenu.IsOpen = true;
                RightClickedVertice = v;
            }

            if (e.LeftButton == MouseButtonState.Pressed)
                LeftClickedVertice = v;

            if (FirstVertice == v && !Completed)
                Complete();
        }
        private void RemoveConstraints(Edge edg)
        {
            edg.constraint = null;
        }
        #endregion

        #region AddingConstraints
        private void OnEdgeClicked(object sender, MouseEventArgs e)
        {
            if (Completed)
            {
                Edge edge = sender as Edge;

                if (e.RightButton == MouseButtonState.Pressed)
                {
                    edgeMenu.IsOpen = true;
                    RightClickedEdge = edge;
                }
                else if (e.LeftButton == MouseButtonState.Pressed)
                {
                    AddMidVertice(edge);
                }
            }
        }

        private bool CheckIfHorizontalNeighbour()
        {
            int index = edges.FindIndex(edge => edge == RightClickedEdge);
            Edge nextEdge = edges[(index + 1) % EdgesCount];
            Edge prevEdge = edges[index == 0 ? EdgesCount - 1 : (index - 1) % EdgesCount];
            if ((prevEdge.constraint != null && prevEdge.constraint.GetType() == typeof(HorizontalConstraint))
                || (nextEdge.constraint != null && nextEdge.constraint.GetType() == typeof(HorizontalConstraint)))
            {
                FailInformation("Sąsiadująca krawędź ma już nadany więz poziomy");
                return true;
            }
            return false;
        }
        private void AddHorizontalConstraint(object sender, RoutedEventArgs e)
        {
            if (!InitialCheck())
                return;

            if (CheckIfHorizontalNeighbour())
                return;

            RightClickedEdge.constraint = new HorizontalConstraint(RightClickedEdge);
            ReDraw();

            bool result = RepairConstraints();
            AfterRepair(result);
        }

        private bool CheckIfVerticalNeighbour()
        {
            int index = edges.FindIndex(edge => edge == RightClickedEdge);
            Edge nextEdge = edges[(index + 1) % EdgesCount];
            Edge prevEdge = edges[index == 0 ? EdgesCount - 1 : (index - 1) % EdgesCount];
            if ((prevEdge.constraint != null && prevEdge.constraint.GetType() == typeof(VerticalConstraint))
                 || (nextEdge.constraint != null && nextEdge.constraint.GetType() == typeof(VerticalConstraint)))
            {
                FailInformation("Sąsiadująca krawędź ma już nadany więz");
                return true;
            }
            return false;
        }
        private void AddVerticalConstraint(object sender, RoutedEventArgs e)
        {
            if (!InitialCheck())
                return;

            if (CheckIfVerticalNeighbour())
                return;

            RightClickedEdge.constraint = new VerticalConstraint(RightClickedEdge);
            bool result = RepairConstraints();

            AfterRepair(result);
        }

        private bool getLengthInput(out double length)
        {
            var dialog = new InputDialog();
            length = RightClickedEdge.Length;
            if (dialog.ShowDialog() == true)
            {
                if (!double.TryParse(dialog.LengthValue.Text, out length))
                {
                    FailInformation("Length must be number");
                    return false;
                }
                if(length <= 0)
                {
                    FailInformation("Length must be positive number");
                    return false;
                }
            }
            return true;
        }
        private void AddLenghtConstraint(object sender, RoutedEventArgs e)
        {
            double lengthValue;
            if (!getLengthInput(out lengthValue))
                return;

            if (!InitialCheck())
                return;

            RightClickedEdge.constraint = new LengthConstraint(RightClickedEdge, lengthValue);
            bool result = RepairConstraints();

            AfterRepair(result);
        }
        #endregion

        #region Checking_Repairing_Constraint

        private bool InitialCheck()
        {
            if (RightClickedEdge.HasConstraint)
            {
                FailInformation("Krawędź ma już nadany więz");
                return false;
            }
            MakeSnapshot();
            return true;
        }

        private void AfterRepair(bool checkResult)
        {
            if (checkResult)
                ReDraw();
            else
            {
                RightClickedEdge.constraint = null;
                UseSnapshot();
                ReDraw();
            }
        }

        private void RepairConstraintsWhileMoving()
        {
            for (int i = 0; i < EdgesCount; i++)
            {
                Edge checkedEdge = edges[i];
                if (checkedEdge.HasConstraint && !checkedEdge.constraint.IsFulfiled())
                {
                    if (checkedEdge.v2 == LeftClickedVertice)
                        checkedEdge.constraint.Fulfil(2);
                    else
                        checkedEdge.constraint.Fulfil();
                }
            }

            foreach (var edge in edges)
            {
                RightClickedEdge = edge;
                RepairConstraints();
                RightClickedEdge = null;
            }
        }

        private bool RepairConstraints()
        {
            Edge edg = RightClickedEdge;
            int index = edges.FindIndex(e => e == edg);
            int i = index;
            while (true)
            {
                Edge nextEdge = edges[(i + 1) % EdgesCount];
                if (nextEdge.HasConstraint && !nextEdge.constraint.IsFulfiled())
                {
                    nextEdge.constraint.Fulfil();
                    if (!CheckIfLengthsAreCorrect())
                    {
                        FailInformation("Nie można dodać więzu - zaburzyłby istniejące więzy");
                        return false;
                    }
                }
                else
                    return true;
                i++;
            }
        }

        private bool CheckIfLengthsAreCorrect()
        {
            double sum = 0;
            foreach (var e in edges)
                sum += e.Length;

            foreach (var e in edges)
                if (e.Length > sum - e.Length)
                    return false;
            return true;
        }
        #endregion
    }
}
