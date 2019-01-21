using System.Windows.Media;

namespace PolygonEditor.Model
{
    public class HorizontalConstraint : Constraint
    {
        public HorizontalConstraint(Edge e) : base(e)
        {
            Fulfil();
        }

        public override Brush Color => Brushes.Purple;

        public override bool IsFulfiled()
        {
            return edge.v1.point.Y == edge.v2.point.Y;
        }

        public override bool Fulfil(int verticeNo = 1)
        {
            if (verticeNo == 2)
                edge.v1.point.Y = edge.v2.point.Y;
            else
                edge.v2.point.Y = edge.v1.point.Y;
            return true;
        }

    }
}
