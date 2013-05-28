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
        private double _a, _b, _yk0, _xkOffset, _xCorrectionShortening;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a">left side mapping proportion</param>
        /// <param name="b">right side mapping proportion</param>
        public IntegralPointMapper(Grid griddata)
            : base(griddata)
        {
            _a = _calculateParameterA(Grid.PresentationQuad, Grid.BeamerQuad);
            _b = _calculateParameterB(Grid.PresentationQuad, Grid.BeamerQuad);
            _yk0 = _calculateOffsetYk(Grid.PresentationQuad);

            // Correct X Stretching
            _xkOffset = (Grid.PresentationQuad.TopLeft.X + Grid.PresentationQuad.BottomLeft.X) / 2;
            double xkMeanMaxOffset = (Grid.PresentationQuad.TopRight.X + Grid.PresentationQuad.BottomRight.X) / 2;
            _xCorrectionShortening = _cameraXToBeamerX(xkMeanMaxOffset - _xkOffset, _a, _b);
        }

        /// <summary>
        /// Get Beamer coordinates from presented picture taken by a camera.
        /// </summary>
        /// <param name="presentation">Measured mapped point</param>
        /// <returns>corresponding estimated beamer point</returns>
        public override Point FromPresentation(Point presentation)
        {
            // calculate beamer x
            double xb = _cameraXToBeamerX(presentation.X - _xkOffset, _a, _b) / _xCorrectionShortening;
            // calculate beamer y
            double yb = _cameraYToBeamerY(presentation.Y - Grid.PresentationQuad.TopRight.Y, xb, _a, _b, _yk0);
            
            // correct coordinates to a quadliteral TODO
            //Point corrected = _barycentricCorrectedPoint(new Point((float)xb, (float)yb), Grid.PresentationQuad);

            Point corrected = new Point((float)xb, (float)yb);
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

        #endregion

        #region Environmental Parameters

        /// <summary>
        /// Calculate left shortening factor A.
        /// </summary>
        /// <param name="from">Source Corners</param>
        /// <param name="to">Traget Corners</param>
        /// <returns></returns>
        private double _calculateParameterA(Quad from, Quad to)
        {
            double distFrom = from.TopLeft.DistanceTo(from.BottomLeft);
            double distTo = to.TopLeft.DistanceTo(to.BottomLeft);
            return distFrom / distTo;
        }

        /// <summary>
        /// Calculate right shortening factor B
        /// </summary>
        /// <param name="from">Source Corners</param>
        /// <param name="to">Target Corners</param>
        /// <returns></returns>
        private double _calculateParameterB(Quad from, Quad to)
        {
            double distFrom = from.TopRight.DistanceTo(from.BottomRight);
            double distTo = to.TopRight.DistanceTo(to.BottomRight);
            return distFrom / distTo;
        }

        /// <summary>
        /// Calculates y distance between left and right vertical presentation borders
        /// </summary>
        /// <param name="corners">Reference Quadliteral</param>
        /// <returns></returns>
        private static float _calculateOffsetYk(Quad corners)
        {
            return corners.TopLeft.Y - corners.TopRight.Y;
        }

        #endregion
    }
}
