using AForge;
using System;

namespace HSR.PresWriter.PenTracking.Mappers
{
    internal class IntegralPointMapper : AbstractPointMapper
    {
        private readonly double _a;
        private readonly double _b;
        private readonly double _yk0;
        private readonly double _xkOffset;
        private readonly double _xCorrectionShortening;

        /// <summary>
        /// If set to true, beamer screensize is adjusted to 0 to 1 (Square).
        /// Default is set to false, results then are scaled up to beamer screen size.
        /// </summary>
        private readonly bool _useNormSource;

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="griddata"></param>
        /// <param name="useNormSource"></param>
        public IntegralPointMapper(Grid griddata, bool useNormSource = false)
            : base(griddata)
        {
            _useNormSource = useNormSource;

            _a = _calculateParameterA(Grid.PresentationQuad, Quad.FromUnitSquare());
            _b = _calculateParameterB(Grid.PresentationQuad, Quad.FromUnitSquare());
            _yk0 = _calculateOffsetYk(Grid.PresentationQuad);

            // Correct X Stretching
            _xkOffset = (Grid.PresentationQuad.TopLeft.X + Grid.PresentationQuad.BottomLeft.X) / 2;
            double xkMeanMaxOffset = (Grid.PresentationQuad.TopRight.X + Grid.PresentationQuad.BottomRight.X) / 2;
            _xCorrectionShortening = _cameraXToBeamerX(xkMeanMaxOffset - _xkOffset, _a, _b);
        }

        /// <summary>
        /// Calculate Beamer coordinates from presented picture taken by a camera.
        /// </summary>
        /// <param name="presentation">Measured mapped point</param>
        /// <returns>corresponding estimated beamer point</returns>
        public override Point FromPresentation(Point presentation)
        {
            // calculate beamer x and beamer y (from 0 to 1)
            double xb = _cameraXToBeamerX(presentation.X - _xkOffset, _a, _b) / _xCorrectionShortening;
            // correct extreme distortion by a square function (y also becomes better)
            double c = (_a / _b -1)/10; // TODO bloss Näherungswert. Korrektur sollte über Mittelpunktabweichung erfolgen
            xb -= _correctBySquareFunction(xb, c); // TODO evaluate correction automatically
            double yb = _cameraYToBeamerY(presentation.Y - Grid.PresentationQuad.TopRight.Y, xb, _a, _b, _yk0);

            if (!_useNormSource)
            {
                // scale coordinates up to beamer resolution
                xb *= Grid.BeamerQuad.TopRight.X - Grid.BeamerQuad.TopLeft.X;
                yb *= Grid.BeamerQuad.BottomLeft.Y - Grid.BeamerQuad.TopLeft.Y;
            }
            return new Point((float)xb, (float)yb);
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

        /// <summary>
        /// Correct a function by a square function between 0 and 1.
        /// Maximum of correction function lies at x=0.5 and y=correction
        /// </summary>
        /// <param name="xb">value to correct</param>
        /// <param name="maxCorrection"></param>
        /// <returns>corrected xb</returns>
        private static double _correctBySquareFunction(double xb, double maxCorrection)
        {
            return -4 * maxCorrection * xb * xb + 4 * maxCorrection * xb;
        }

        #endregion

        #region Environmental Parameters

        /// <summary>
        /// Calculate left shortening factor A.
        /// </summary>
        /// <param name="from">Source Corners</param>
        /// <param name="to">Traget Corners</param>
        /// <returns></returns>
        private static double _calculateParameterA(Quad from, Quad to)
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
        private static double _calculateParameterB(Quad from, Quad to)
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
