using System;
using System.Windows;
using System.Windows.Media;

namespace PolygonEditor
{
    public static class Helpers
    {
        public static void BresenhamLine(int X1, int y1, int X2, int y2, GeometryGroup gg)
        {
            // zmienne pomocnicze
            int d, dX, dy, ai, bi, Xi, yi;
            int X = X1, y = y1;
            // ustalenie kierunku rysowania
            if (X1 < X2)
            {
                Xi = 1;
                dX = X2 - X1;
            }
            else
            {
                Xi = -1;
                dX = X1 - X2;
            }
            // ustalenie kierunku rysowania
            if (y1 < y2)
            {
                yi = 1;
                dy = y2 - y1;
            }
            else
            {
                yi = -1;
                dy = y1 - y2;
            }
            // pierwszy piksel
            PutPiXel(X, y, gg);
            // oś wiodąca OX
            if (dX > dy)
            {
                ai = (dy - dX) * 2;
                bi = dy * 2;
                d = bi - dX;
                // pętla po kolejnych X
                while (X != X2)
                {
                    // test współczynnika
                    if (d >= 0)
                    {
                        X += Xi;
                        y += yi;
                        d += ai;
                    }
                    else
                    {
                        d += bi;
                        X += Xi;
                    }
                    PutPiXel(X, y, gg);
                }
            }
            // oś wiodąca OY
            else
            {
                ai = (dX - dy) * 2;
                bi = dX * 2;
                d = bi - dy;
                // pętla po kolejnych y
                while (y != y2)
                {
                    // test współczynnika
                    if (d >= 0)
                    {
                        X += Xi;
                        y += yi;
                        d += ai;
                    }
                    else
                    {
                        d += bi;
                        y += yi;
                    }
                    PutPiXel(X, y, gg);
                }
            }
        }

        private static void PutPiXel(int X, int y, GeometryGroup gg)
        {
            RectangleGeometry piXel = new RectangleGeometry();
            Rect rct = new Rect();
            rct.X = X;
            rct.Y = y;
            rct.Width = 1;
            rct.Height = 1;
            piXel.Rect = rct;
            gg.Children.Add(piXel);
        }

        public static bool withinEpsilon(double a, double b)
        {
            double eps = 0.001;
            return Math.Abs(a - b) < eps;
        }
    }
}
