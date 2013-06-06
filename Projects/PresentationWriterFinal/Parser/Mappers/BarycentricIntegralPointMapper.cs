using AForge;
using System;

namespace HSR.PresWriter.PenTracking.Mappers
{
    internal class BarycentricIntegralPointMapper : IntegralPointMapper
    {
        // per Integral estimated camera coordinates (mapped from calculated )
        private readonly Quad _bariycentricEstimatedCameraCorners;

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
            _bariycentricEstimatedCameraCorners = new Quad
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
            var x = point.X;
            var y = point.Y;
            var result = reference.TopLeft * (1 - x) * (1 - y)
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
            var px = point.X; 
            var py = point.Y;

            double lUx = reference.TopLeft.X;     
            double lUy = reference.TopLeft.Y;
            double lLx = reference.BottomLeft.X;  
            double lLy = reference.BottomLeft.Y;
            double rUx = reference.TopRight.X;    
            double rUy = reference.TopRight.Y;
            double rLx = reference.BottomRight.X; 
            double rLy = reference.BottomRight.Y;

            double xh = (2 * lLy * lUx - 2 * lLx * lUy
                - lLy * px + lUy * px + lLx * py - lUx * py
                + lUy * rLx - py * rLx - lUx * rLy + px * rLy
                - lLy * rUx + py * rUx + lLx * rUy - px * rUy
                + Math.Sqrt(
                    -4 * (lLy * (lUx - px) + lUy * px - lUx * py + lLx * (-lUy + py)) * ((lLy - rLy) * (lUx - rUx) - (lLx - rLx) * (lUy - rUy))
                    + Math.Pow(
                        -py * rLx + lUy * (px + rLx) + px * rLy -
                        lUx * (py + rLy) + lLy * (2 * lUx - px - rUx) + py * rUx - px * rUy +
                        lLx * (-2 * lUy + py + rUy), 2)
                )
            ) / (2 * ((lLy - rLy) * (lUx - rUx) - (lLx - rLx) * (lUy - rUy)));

            double yh = (lLy * px - lUy * px - lLx * py + lUx * py - lUy * rLx + py * rLx + lUx * rLy -
                px * rLy - lLy * rUx + 2 * lUy * rUx - py * rUx + lLx * rUy - 2 * lUx * rUy +
                px * rUy + Math.Sqrt(-4 * ((lLy - lUy) * (rLx - rUx) - (lLx - lUx) * (rLy -
                rUy)) * (-py * rUx + lUy * (-px + rUx) + lUx * (py - rUy) +
                px * rUy) + Math.Pow(-lLx * py + lUx * py + py * rLx + lUx * rLy - px * rLy -
                lUy * (px + rLx - 2 * rUx) + lLy * (px - rUx) -
                py * rUx + (lLx - 2 * lUx + px) * rUy, 2))) / (2 * ((lLy - lUy) * (rLx -
                rUx) - (lLx - lUx) * (rLy - rUy)));

            return new Point((float)xh, (float)yh);
        }

        #endregion
    }
}
