using AForge;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSR.PresWriter.PenTracking
{
    public struct Tetragon
    {
        public Point LeftUpper { get; set; }
        public Point LeftLower { get; set; }
        public Point RightUpper { get; set; }
        public Point RightLower { get; set; }
    }

    public class IntegralPointMapper : AbstractPointMapper
    {
        private static double _beamerXToCameraX(double xb, double a, double b)
        {
            return 0.5 * (a - b) * xb * xb + a * xb;
        }

        private static double _cameraXToBeamerX(double xk, double a, double b)
        {
            return (-a + Math.Sqrt(a * a + 2 * (b - a) * xk)) / (b - a);
        }

        private static double _beamerYToCameraY(double xb, double yb, double a, double b, double yk0)
        {
            return (a * yb + yk0) * (1 - xb) + b * yb * xb;
        }

        private static double _cameraYToBeamerY(double yk, double xb, double a, double b, double yk0)
        {
            return (yk - yk0 + yk0 * xb) / (a - a * xb + b * xb);
        }

        private static Point barycentricCorrectedPoint(Point point, Tetragon reference)
        {
            float x = point.X;
            float y = point.Y;
            Point result = reference.LeftUpper * (1 - x) * (1 - y)
                + reference.RightUpper * (1 - x) * y
                + reference.LeftLower * x * (1 - y)
                + reference.RightLower * x * y;
            return result;
        }

        private double _a, _b;
        private Tetragon _camera, _beamer;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a">left side mapping proportion</param>
        /// <param name="b">right side mapping proportion</param>
        public IntegralPointMapper(double a, double b): base(null)
        {
            _a = a;
            _b = b;
        }

        public override Point FromPresentation(Point p)
        {
            return new Point();
        }

        public Point ToCameraPoint(Point beamerPoint)
        {

            return new Point();
        }
    }
}
