using AForge;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSR.PresWriter.PenTracking
{
    public class IntegralPointMapper : AbstractPointMapper
    {
        private double _a, _b, _yk0;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a">left side mapping proportion</param>
        /// <param name="b">right side mapping proportion</param>
        public IntegralPointMapper(Grid griddata, double a, double b)
            : base(griddata)
        {
            _a = a;
            _b = b;
            _yk0 = 0;
        }

        /// <summary>
        /// Get Beamer coordinates from presented picture taken by a camera
        /// </summary>
        /// <param name="presentation"></param>
        /// <returns></returns>
        public override Point FromPresentation(Point presentation)
        {
            double xb = _cameraXToBeamerX(presentation.X, _a, _b);
            double yb = _cameraYToBeamerY(presentation.Y, xb, _a, _b, _yk0);
            Point corrected = _barycentricCorrectedPoint(new Point((float)xb, (float)yb), Grid.PresentationQuad);
            return corrected;
        }

        #region Mapping Math

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

        private static Point _barycentricCorrectedPoint(Point point, Quad reference)
        {
            float x = point.X;
            float y = point.Y;
            Point result = reference.TopLeft * (1 - x) * (1 - y)
                + reference.TopRight * (1 - x) * y
                + reference.BottomLeft * x * (1 - y)
                + reference.BottomRight * x * y;
            return result;
        }

        private static int _calculateOffsetYk(Quad corners)
        {
            return 0;
        }

        #endregion
    }
}
