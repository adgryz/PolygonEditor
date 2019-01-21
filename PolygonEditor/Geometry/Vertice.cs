using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace PolygonEditor.Model
{
    public class Vertice
    {
        public Vertice(Point point, MouseEventHandler clickHandler, Canvas canvas)
        {
            this.point = point;
            this.canvas = canvas;
            Clicked += clickHandler;
        }

        public Point point;
        public Brush color = Brushes.Black;
        public event MouseEventHandler Clicked;
        public void Draw()
        {
            int height = circleSize;
            int width = circleSize;
            double left = point.X - width / 2;
            double top = point.Y - height / 2;

            circle = new Ellipse { Height = height, Width = width };
            circle.Margin = new Thickness(left, top, 0, 0);
            circle.Fill = color;
            circle.MouseDown += TriggerVerticeClick;
            canvas.Children.Add(circle);
        }

        private Ellipse circle;
        private Canvas canvas;
        private int circleSize = 8;
        private void Delete(object sender, EventArgs e)
        {
            Console.WriteLine("Delte");
        }
        private void TriggerVerticeClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (Clicked != null && e.LeftButton == MouseButtonState.Pressed  || e.RightButton == MouseButtonState.Pressed)
                Clicked(this, e);
        }
    }
}
