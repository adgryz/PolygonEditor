using System.Windows.Media;

namespace PolygonEditor.Model
{
    class LengthConstraint : Constraint
    {
        public LengthConstraint(Edge e, double length = 200) : base(e)
        {
            Length = length;
            Fulfil();
        }

        public double Length;

        public override Brush Color => Brushes.Pink;

        public override bool IsFulfiled()
        {
            return Helpers.withinEpsilon(Length, edge.Length);
        }

        public override bool Fulfil(int verticeNo = 1)
        {
            if (verticeNo == 2)
            {
                double x1 = edge.v1.point.X;
                double x2 = edge.v2.point.X;
                double y1 = edge.v1.point.Y;
                double y2 = edge.v2.point.Y;

                edge.v1.point.X = x2 + (Length / edge.Length) * (x1 - x2);
                edge.v1.point.Y = y2 + (Length / edge.Length) * (y1 - y2);
            }
            else
            {
                double x1 = edge.v1.point.X;
                double x2 = edge.v2.point.X;
                double y1 = edge.v1.point.Y;
                double y2 = edge.v2.point.Y;

                edge.v2.point.X = x1 + (Length / edge.Length) * (x2 - x1);
                edge.v2.point.Y = y1 + (Length / edge.Length) * (y2 - y1);
            }

            return true;
        }
    }
}
