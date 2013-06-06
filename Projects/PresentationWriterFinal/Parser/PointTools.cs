using System;
using System.Drawing;

namespace HSR.PresWriter.PenTracking
{
    public static class PointTools
    {
        public static Point CalculateCenterPoint(Rectangle r)
        {
            return new Point(r.X + r.Width / 2, r.Y + r.Height);
        }

        public static double CalculateDistance(Point a, Point b)
        {
            return Math.Sqrt(Math.Pow(b.X - a.X, 2) + Math.Pow(b.Y - a.Y, 2));
        }

        public static Point CalculateCenterPoint(Point a, Point b)
        {
            // Always floor the results, thats conservative
            var x = a.X + ((b.X - a.X) / 2);
            var y = a.Y + ((b.Y - a.Y) / 2);
            return new Point(x, y);
        }

        public static Point CalculateIntermediatePoint(Point a, Point b, double ratio = 0.5)
        {
            var x = (int)Math.Round(a.X + ((b.X - a.X) * ratio));
            var y = (int)Math.Round(a.Y + ((b.Y - a.Y) * ratio));
            return new Point(x, y);
        }

    }
}
