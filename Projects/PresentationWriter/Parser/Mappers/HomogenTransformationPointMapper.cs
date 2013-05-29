using System;
using System.Collections.Generic;
using AForge;

namespace HSR.PresWriter.PenTracking.Mappers
{
    public class HomogenTransformationPointMapper : AbstractPointMapper
    {
        private const double TOLERANCE = 1e-13;
        private Point[,] _mapping;


        // Caclculates determinant of a 2x2 matrix
        private static double Det2(double a, double b, double c, double d)
        {
            return (a*d - b*c);
        }

        // Multiply two 3x3 matrices
        private static double[,] MultiplyMatrix(double[,] a, double[,] b)
        {
            double[,] c = new double[3,3];

            c[0, 0] = a[0, 0]*b[0, 0] + a[0, 1]*b[1, 0] + a[0, 2]*b[2, 0];
            c[0, 1] = a[0, 0]*b[0, 1] + a[0, 1]*b[1, 1] + a[0, 2]*b[2, 1];
            c[0, 2] = a[0, 0]*b[0, 2] + a[0, 1]*b[1, 2] + a[0, 2]*b[2, 2];
            c[1, 0] = a[1, 0]*b[0, 0] + a[1, 1]*b[1, 0] + a[1, 2]*b[2, 0];
            c[1, 1] = a[1, 0]*b[0, 1] + a[1, 1]*b[1, 1] + a[1, 2]*b[2, 1];
            c[1, 2] = a[1, 0]*b[0, 2] + a[1, 1]*b[1, 2] + a[1, 2]*b[2, 2];
            c[2, 0] = a[2, 0]*b[0, 0] + a[2, 1]*b[1, 0] + a[2, 2]*b[2, 0];
            c[2, 1] = a[2, 0]*b[0, 1] + a[2, 1]*b[1, 1] + a[2, 2]*b[2, 1];
            c[2, 2] = a[2, 0]*b[0, 2] + a[2, 1]*b[1, 2] + a[2, 2]*b[2, 2];

            return c;
        }

        // Calculates adjugate 3x3 matrix
        private static double[,] AdjugateMatrix(double[,] a)
        {
            double[,] b = new double[3,3];
            b[0, 0] = Det2(a[1, 1], a[1, 2], a[2, 1], a[2, 2]);
            b[1, 0] = Det2(a[1, 2], a[1, 0], a[2, 2], a[2, 0]);
            b[2, 0] = Det2(a[1, 0], a[1, 1], a[2, 0], a[2, 1]);
            b[0, 1] = Det2(a[2, 1], a[2, 2], a[0, 1], a[0, 2]);
            b[1, 1] = Det2(a[2, 2], a[2, 0], a[0, 2], a[0, 0]);
            b[2, 1] = Det2(a[2, 0], a[2, 1], a[0, 0], a[0, 1]);
            b[0, 2] = Det2(a[0, 1], a[0, 2], a[1, 1], a[1, 2]);
            b[1, 2] = Det2(a[0, 2], a[0, 0], a[1, 2], a[1, 0]);
            b[2, 2] = Det2(a[0, 0], a[0, 1], a[1, 0], a[1, 1]);

            return b;
        }


        private static double[,] MapSquareToQuad(List<IntPoint> quad)
        {
            double[,] sq = new double[3,3];
            double px, py;

            px = quad[0].X - quad[1].X + quad[2].X - quad[3].X;
            py = quad[0].Y - quad[1].Y + quad[2].Y - quad[3].Y;

            if ((px < TOLERANCE) && (px > -TOLERANCE) &&
                (py < TOLERANCE) && (py > -TOLERANCE))
            {
                sq[0, 0] = quad[1].X - quad[0].X;
                sq[0, 1] = quad[2].X - quad[1].X;
                sq[0, 2] = quad[0].X;

                sq[1, 0] = quad[1].Y - quad[0].Y;
                sq[1, 1] = quad[2].Y - quad[1].Y;
                sq[1, 2] = quad[0].Y;

                sq[2, 0] = 0.0;
                sq[2, 1] = 0.0;
                sq[2, 2] = 1.0;
            }
            else
            {
                double dx1, dx2, dy1, dy2, del;

                dx1 = quad[1].X - quad[2].X;
                dx2 = quad[3].X - quad[2].X;
                dy1 = quad[1].Y - quad[2].Y;
                dy2 = quad[3].Y - quad[2].Y;

                del = Det2(dx1, dx2, dy1, dy2);

                if (del == 0.0)
                    return null;

                sq[2, 0] = Det2(px, dx2, py, dy2)/del;
                sq[2, 1] = Det2(dx1, px, dy1, py)/del;
                sq[2, 2] = 1.0;

                sq[0, 0] = quad[1].X - quad[0].X + sq[2, 0]*quad[1].X;
                sq[0, 1] = quad[3].X - quad[0].X + sq[2, 1]*quad[3].X;
                sq[0, 2] = quad[0].X;

                sq[1, 0] = quad[1].Y - quad[0].Y + sq[2, 0]*quad[1].Y;
                sq[1, 1] = quad[3].Y - quad[0].Y + sq[2, 1]*quad[3].Y;
                sq[1, 2] = quad[0].Y;
            }
            return sq;
        }

        // Calculate matrix for general quad to quad mapping
        private static double[,] MapQuadToQuad(List<IntPoint> input, List<IntPoint> output)
        {
            double[,] squareToInpit = MapSquareToQuad(input);
            double[,] squareToOutput = MapSquareToQuad(output);

            if (squareToOutput == null)
                return null;

            return MultiplyMatrix(squareToOutput, AdjugateMatrix(squareToInpit));
        }


        public HomogenTransformationPointMapper(Grid griddata)
            : base(griddata)
        {
            var sourceQuadrilateral = new List<IntPoint>
                {
                    new IntPoint(griddata.TopLeft.X, griddata.TopLeft.Y),
                    new IntPoint(griddata.TopRight.X, griddata.TopRight.Y),
                    new IntPoint(griddata.BottomRight.X, griddata.BottomRight.Y),
                    new IntPoint(griddata.BottomLeft.X, griddata.BottomLeft.Y)
                };

            // get source and destination images size
            var srcWidth = (int) griddata.CameraQuad.BottomRight.X;
            var srcHeight = (int) griddata.CameraQuad.BottomRight.Y;
            var dstWidth = (int) griddata.BeamerQuad.BottomRight.X;
            var dstHeight = (int) griddata.BeamerQuad.BottomRight.Y;

            _mapping = new Point[dstWidth,dstHeight];

            // find equations of four quadrilateral's edges ( f(x) = k*x + b )
            double kTop;
            double _bTop;
            double bLeft;
            double _kLeft;
            double bRight;
            double kRight;
            double _bBottom;
            double kBottom;

            // top edge
            if (sourceQuadrilateral[1].X == sourceQuadrilateral[0].X)
            {
                kTop = 0;
                _bTop = sourceQuadrilateral[1].X;
            }
            else
            {
                kTop = (double) (sourceQuadrilateral[1].Y - sourceQuadrilateral[0].Y)/
                        (sourceQuadrilateral[1].X - sourceQuadrilateral[0].X);
                _bTop = (double) sourceQuadrilateral[0].Y - kTop*sourceQuadrilateral[0].X;
            }

            // bottom edge
            if (sourceQuadrilateral[2].X == sourceQuadrilateral[3].X)
            {
                kBottom = 0;
                _bBottom = sourceQuadrilateral[2].X;
            }
            else
            {
                kBottom = (double) (sourceQuadrilateral[2].Y - sourceQuadrilateral[3].Y)/
                           (sourceQuadrilateral[2].X - sourceQuadrilateral[3].X);
                _bBottom = (double) sourceQuadrilateral[3].Y - kBottom*sourceQuadrilateral[3].X;
            }

            // left edge
            if (sourceQuadrilateral[3].X == sourceQuadrilateral[0].X)
            {
                _kLeft = 0;
                bLeft = sourceQuadrilateral[3].X;
            }
            else
            {
                _kLeft = (double) (sourceQuadrilateral[3].Y - sourceQuadrilateral[0].Y)/
                         (sourceQuadrilateral[3].X - sourceQuadrilateral[0].X);
                bLeft = (double) sourceQuadrilateral[0].Y - _kLeft*sourceQuadrilateral[0].X;
            }

            // right edge
            if (sourceQuadrilateral[2].X == sourceQuadrilateral[1].X)
            {
                kRight = 0;
                bRight = sourceQuadrilateral[2].X;
            }
            else
            {
                kRight = (double) (sourceQuadrilateral[2].Y - sourceQuadrilateral[1].Y)/
                          (sourceQuadrilateral[2].X - sourceQuadrilateral[1].X);
                bRight = (double) sourceQuadrilateral[1].Y - kRight*sourceQuadrilateral[1].X;
            }

            // some precalculated values
            double leftFactor = (double) (sourceQuadrilateral[3].Y - sourceQuadrilateral[0].Y)/dstHeight;
            double rightFactor = (double) (sourceQuadrilateral[2].Y - sourceQuadrilateral[1].Y)/dstHeight;

            var srcY0 = sourceQuadrilateral[0].Y;
            var srcY1 = sourceQuadrilateral[1].Y;

            // source width and height decreased by 1
            int ymax = srcHeight - 1;
            int xmax = srcWidth - 1;

            // for each line
            for (int y = 0; y < dstHeight; y++)
            {
                // find corresponding Y on the left edge of the quadrilateral
                double yHorizLeft = leftFactor*y + srcY0;
                // find corresponding X on the left edge of the quadrilateral
                double xHorizLeft = (_kLeft == 0) ? bLeft : (yHorizLeft - bLeft)/_kLeft;

                // find corresponding Y on the right edge of the quadrilateral
                double yHorizRight = rightFactor*y + srcY1;
                // find corresponding X on the left edge of the quadrilateral
                double xHorizRight = (kRight == 0) ? bRight : (yHorizRight - bRight)/kRight;

                // find equation of the line joining points on the left and right edges
                double kHoriz, bHoriz;

                if (xHorizLeft == xHorizRight)
                {
                    kHoriz = 0;
                    bHoriz = xHorizRight;
                }
                else
                {
                    kHoriz = (yHorizRight - yHorizLeft)/(xHorizRight - xHorizLeft);
                    bHoriz = yHorizLeft - kHoriz*xHorizLeft;
                }

                double horizFactor = (xHorizRight - xHorizLeft)/dstWidth;

                for (int x = 0; x < dstWidth; x++)
                {
                    double xs = horizFactor*x + xHorizLeft;
                    double ys = kHoriz*xs + bHoriz;

                    if (true || (xs >= 0) && (ys >= 0) && (xs < srcWidth) && (ys < srcHeight))
                    {

                    }
                }
            }
        }

        public override Point FromPresentation(Point p)
        {
            return _mapping[(int) Math.Round(p.X), (int) Math.Round(p.Y)];
        }
    }
}
