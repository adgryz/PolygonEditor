using Microsoft.Win32;
using PolygonEditor.Model;
using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
//1b
namespace PolygonEditor
{
    public partial class MainWindow : Window
    {
        MyPolygon polygon;
        MyPolygon secondPolygon;

        bool secondPolygonAllowed;

        bool clickInsidePolygonBoundingBox;
        Point lastMousePosition;

        public MainWindow()
        {
            InitializeComponent();

            polygon = new MyPolygon(paintSurface);
            secondPolygon = new MyPolygon(paintSurface);

            clickInsidePolygonBoundingBox = false;
            secondPolygonAllowed = false;
        }

        private void ReDrawAll()
        {
            this.paintSurface.Children.Clear();
            polygon.ReDraw();
            secondPolygon.ReDraw();
        }

        private void OnCanvasClick(object sender, MouseButtonEventArgs e)
        {
            lastMousePosition = e.GetPosition(paintSurface);

            if (!polygon.Completed)
                polygon.AddVertice(e.GetPosition(paintSurface));
            else
            {
                CheckIfClickInsidePolygon(e);
                if(!secondPolygon.Completed && !Keyboard.IsKeyDown(Key.LeftShift) && secondPolygonAllowed)
                    secondPolygon.AddVertice(e.GetPosition(paintSurface));
            }

            ReDrawAll();
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (polygon.FirstVertice != null && !polygon.Completed)
                polygon.DrawPreviewLine(e.GetPosition(paintSurface));

            if (e.LeftButton == MouseButtonState.Released)
                polygon.LeftClickedVertice = null;

            if (polygon.LeftClickedVertice != null && polygon.Completed &&
                e.LeftButton == MouseButtonState.Pressed && !Keyboard.IsKeyDown(Key.LeftShift))
                MoveVerice(e);

            if (clickInsidePolygonBoundingBox && polygon.Completed &&
                e.LeftButton == MouseButtonState.Pressed && Keyboard.IsKeyDown(Key.LeftShift))
                MovePolygon(e.GetPosition(paintSurface));
            ReDrawAll();
        }

        private void MoveVerice(MouseEventArgs e)
        {
            polygon.MoveVertice(e.GetPosition(paintSurface));
            ReDrawAll();
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
                DeletePolygon();
            ReDrawAll();
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            clickInsidePolygonBoundingBox = false;
            ReDrawAll();
        }

        private void DeletePolygon()
        {
            paintSurface.Children.Clear();
            polygon = new MyPolygon(paintSurface);
            secondPolygon = new MyPolygon(paintSurface);
            ReDrawAll();
        }

        private void MovePolygon(Point actualMousePosition)
        {
            int dx = (int)(actualMousePosition.X - lastMousePosition.X);
            int dy = (int)(actualMousePosition.Y - lastMousePosition.Y);
            lastMousePosition = actualMousePosition;

            polygon.Move(dx, dy);
            ReDrawAll();
        }

        private void CheckIfClickInsidePolygon(MouseButtonEventArgs e)
        {
            Rect boundingBox = polygon.GetBoundingBox();
            Point click = e.GetPosition(paintSurface);
            if (click.X < boundingBox.BottomRight.X && click.Y < boundingBox.BottomRight.Y
                    && click.X > boundingBox.TopLeft.X && click.Y > boundingBox.TopLeft.Y)
                clickInsidePolygonBoundingBox = true;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            secondPolygonAllowed = true;
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            string result = "";
            if (openFileDialog.ShowDialog() == true)
            {
                result = File.ReadAllText(openFileDialog.FileName);
                DeletePolygon();
                polygon = new MyPolygon(paintSurface);
                int lines = CountLines(result);
                polygon.Deserialize(result);
                    ReDrawAll();
            }

        }
        private int CountLines(string str)
        {
            if (str == null)
                throw new ArgumentNullException("str");
            if (str == string.Empty)
                return 0;
            int index = -1;
            int count = 0;
            while (-1 != (index = str.IndexOf(Environment.NewLine, index + 1)))
                count++;

            return count + 1;
        }

        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Text file (*.txt)|*.txt";
            string polygonStr = polygon.Serialize();
            if (saveFileDialog.ShowDialog() == true)
                File.WriteAllText(saveFileDialog.FileName, polygonStr);
        }
    }
}
