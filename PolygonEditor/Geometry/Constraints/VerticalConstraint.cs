using System.Windows.Media;

namespace PolygonEditor.Model
{
    public class VerticalConstraint : Constraint
    {
        public VerticalConstraint(Edge e) : base(e)
        {
            Fulfil();
        }

        public override Brush Color => Brushes.Orange;

        public override bool IsFulfiled()
        {
            return edge.v1.point.X == edge.v2.point.X;
        }

        public override bool Fulfil(int verticeNo = 1)
        {
            if (verticeNo == 2)
                edge.v1.point.X = edge.v2.point.X;
            else
                edge.v2.point.X = edge.v1.point.X;
            return true;
        }
    }
}
