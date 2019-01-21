using System.Windows.Media;

namespace PolygonEditor.Model
{
    public abstract class Constraint
    {
        public Edge edge;
        public Constraint(Edge e)
        {
           this.edge = e;
        }
        public abstract bool IsFulfiled();
        public abstract bool Fulfil(int verticeNo = 1);
        public abstract Brush Color { get;  }
    }
}
