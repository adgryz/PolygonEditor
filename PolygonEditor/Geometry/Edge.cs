using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace PolygonEditor.Model
{
    public class Edge
    {
        public Edge(Vertice u1, Vertice u2, MouseEventHandler clickHandler, Canvas canvas)
        {
            v1 = u1;
            v2 = u2;
            this.canvas = canvas;
            Clicked += clickHandler;
        }

        public Vertice v1;
        public Vertice v2;
        public Constraint constraint;

        public Brush color = Brushes.Black;
        public event MouseEventHandler Clicked;
        public double Length { get => Math.Sqrt(Math.Pow((v1.point.X - v2.point.X), 2) + Math.Pow((v1.point.Y - v2.point.Y), 2)); }
        public bool HasConstraint { get => constraint != null; }

        private GeometryGroup geometryGroup;
        private Canvas canvas;
        private Path line;

        public void Draw()
        {
            line = new Path();
            geometryGroup = new GeometryGroup();
            line.Fill = color;
            if (HasConstraint)
                line.Fill = constraint.Color;

            line.MouseDown += TriggerEdgeClick;
            Helpers.BresenhamLine((int)v1.point.X, (int)v1.point.Y, (int)v2.point.X, (int)v2.point.Y, geometryGroup);
            line.Data = geometryGroup;
            canvas.Children.Add(line);

            if (HasConstraint)
                DrawConstraintIcon();
        }
        private void DrawConstraintIcon()
        {
            Rectangle icon = new Rectangle();
            icon.Width = 10;
            icon.Height = 10;
            icon.Fill = constraint.Color;

            double left = Math.Min(v1.point.X, v2.point.X) + Math.Abs(v1.point.X - v2.point.X) / 2 + 10;
            double top = Math.Min(v1.point.Y, v2.point.Y) + Math.Abs(v1.point.Y - v2.point.Y) / 2 + 10;
            icon.Margin = new Thickness(left, top, 0, 0);
            canvas.Children.Add(icon);
        }
        private void TriggerEdgeClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (Clicked != null && e.LeftButton == MouseButtonState.Pressed || e.RightButton == MouseButtonState.Pressed)
                Clicked(this, e);
        }
    }
}
