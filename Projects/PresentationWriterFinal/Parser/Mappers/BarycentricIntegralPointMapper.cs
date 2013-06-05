using AForge;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSR.PresWriter.PenTracking.Mappers
{
    public class BarycentricIntegralPointMapper : IntegralPointMapper
    {
        // per Integral estimated camera coordinates (mapped from calculated )
        private Quad _bariycentricEstimatedCameraCorners;

        public BarycentricIntegralPointMapper(Grid griddata)
            : base(griddata, true)
        {
            // Calculate estimated beamer coordinates with integral approach
            Point beamerTopLeft     = base.FromPresentation(Grid.PresentationQuad.TopLeft);
            Point beamerTopRight    = base.FromPresentation(Grid.PresentationQuad.TopRight);
            Point beamerBottomLeft  = base.FromPresentation(Grid.PresentationQuad.BottomLeft);
            Point beamerBottomRight = base.FromPresentation(Grid.PresentationQuad.BottomRight);

            // Process barycentric mapping back to presentation coordinates (beamer picture on camera)
            // This Corners are the new reference for a more precise mapping
            _bariycentricEstimatedCameraCorners = new Quad()
            {
                TopLeft     = _barycentricCorrectionFromSquare(beamerTopLeft, Grid.PresentationQuad),
                TopRight    = _barycentricCorrectionFromSquare(beamerTopRight, Grid.PresentationQuad),
                BottomLeft  = _barycentricCorrectionFromSquare(beamerBottomLeft, Grid.PresentationQuad),
                BottomRight = _barycentricCorrectionFromSquare(beamerBottomRight, Grid.PresentationQuad)
            };
        }

        /// <summary>
        /// Get beamer mapping calculated by integral approach and corrected with barycentric stretching
        /// </summary>
        /// <param name="presentation">Measured Point, which is about to be mapped</param>
        /// <returns>Mapped point on source surface (Beamer)</returns>
        public override Point FromPresentation(Point presentation)
        {
            // Calculate beamer coordinates per integral approach
            Point naiveBeamerPoint = base.FromPresentation(presentation);
            // Correct them with previously calculated barycentric corrected corners
            Point estimatedCameraPoint = _barycentricCorrectionFromSquare(naiveBeamerPoint, Grid.PresentationQuad);
            Point correctedBeamerPoint = _barycentricCorrectionToSquare(estimatedCameraPoint, _bariycentricEstimatedCameraCorners);

            // scale coordinates up to beamer resolution
            correctedBeamerPoint.X *= Grid.BeamerQuad.TopRight.X - Grid.BeamerQuad.TopLeft.X;
            correctedBeamerPoint.Y *= Grid.BeamerQuad.BottomLeft.Y - Grid.BeamerQuad.TopLeft.Y;
            return correctedBeamerPoint;
        }

        #region Barycentric Correction Math

        /// <summary>
        /// Barycentric correction of square coordinates custom quad coordinates
        /// </summary>
        /// <param name="point"></param>
        /// <param name="reference"></param>
        /// <returns></returns>
        public static Point _barycentricCorrectionFromSquare(Point point, Quad reference)
        {
            float x = point.X;
            float y = point.Y;
            Point result = reference.TopLeft * (1 - x) * (1 - y)
                + reference.BottomLeft * y * (1 - x)
                + reference.TopRight * (1 - y) * x
                + reference.BottomRight * x * y;
            return result;
        }

        /// <summary>
        /// 
        /// Inverse Function of _barycentricCorrectionFromSquare
        /// </summary>
        /// <param name="point"></param>
        /// <param name="reference"></param>
        /// <returns></returns>
        public static Point _barycentricCorrectionToSquare(Point point, Quad reference)
        {
            double px, py;
            px = point.X; py = point.Y;

            double LUx, LLx, RUx, RLx, LUy, LLy, RUy, RLy;
            LUx = reference.TopLeft.X;     LUy = reference.TopLeft.Y;
            LLx = reference.BottomLeft.X;  LLy = reference.BottomLeft.Y;
            RUx = reference.TopRight.X;    RUy = reference.TopRight.Y;
            RLx = reference.BottomRight.X; RLy = reference.BottomRight.Y;

            double xh = (2 * LLy * LUx - 2 * LLx * LUy
                - LLy * px + LUy * px + LLx * py - LUx * py
                + LUy * RLx - py * RLx - LUx * RLy + px * RLy
                - LLy * RUx + py * RUx + LLx * RUy - px * RUy
                + Math.Sqrt(
                    -4 * (LLy * (LUx - px) + LUy * px - LUx * py + LLx * (-LUy + py)) * ((LLy - RLy) * (LUx - RUx) - (LLx - RLx) * (LUy - RUy))
                    + Math.Pow(
                        -py * RLx + LUy * (px + RLx) + px * RLy -
                        LUx * (py + RLy) + LLy * (2 * LUx - px - RUx) + py * RUx - px * RUy +
                        LLx * (-2 * LUy + py + RUy), 2)
                )
            ) / (2 * ((LLy - RLy) * (LUx - RUx) - (LLx - RLx) * (LUy - RUy)));

            double yh = (LLy * px - LUy * px - LLx * py + LUx * py - LUy * RLx + py * RLx + LUx * RLy -
                px * RLy - LLy * RUx + 2 * LUy * RUx - py * RUx + LLx * RUy - 2 * LUx * RUy +
                px * RUy + Math.Sqrt(-4 * ((LLy - LUy) * (RLx - RUx) - (LLx - LUx) * (RLy -
                RUy)) * (-py * RUx + LUy * (-px + RUx) + LUx * (py - RUy) +
                px * RUy) + Math.Pow(-LLx * py + LUx * py + py * RLx + LUx * RLy - px * RLy -
                LUy * (px + RLx - 2 * RUx) + LLy * (px - RUx) -
                py * RUx + (LLx - 2 * LUx + px) * RUy, 2))) / (2 * ((LLy - LUy) * (RLx -
                RUx) - (LLx - LUx) * (RLy - RUy)));

            return new Point((float)xh, (float)yh);
        }

        #endregion
    }
}
