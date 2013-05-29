using AForge;

namespace HSR.PresWriter.PenTracking.Mappers
{
    class HomogenTransformationPointMapper : AbstractPointMapper
    {
        private static double[,] MapSquareToQuad(List<IntPoint> quad)
        {
            double[,] sq = new double[3, 3];
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

                sq[2, 0] = Det2(px, dx2, py, dy2) / del;
                sq[2, 1] = Det2(dx1, px, dy1, py) / del;
                sq[2, 2] = 1.0;

                sq[0, 0] = quad[1].X - quad[0].X + sq[2, 0] * quad[1].X;
                sq[0, 1] = quad[3].X - quad[0].X + sq[2, 1] * quad[3].X;
                sq[0, 2] = quad[0].X;

                sq[1, 0] = quad[1].Y - quad[0].Y + sq[2, 0] * quad[1].Y;
                sq[1, 1] = quad[3].Y - quad[0].Y + sq[2, 1] * quad[3].Y;
                sq[1, 2] = quad[0].Y;
            }
            return sq;
        }

        // Calculate matrix for general quad to quad mapping
        public static double[,] MapQuadToQuad(List<IntPoint> input, List<IntPoint> output)
        {
            double[,] squareToInpit = MapSquareToQuad(input);
            double[,] squareToOutput = MapSquareToQuad(output);

            if (squareToOutput == null)
                return null;

            return MultiplyMatrix(squareToOutput, AdjugateMatrix(squareToInpit));
        }

        private double[,] _matrix;

        public HomogenTransformationPointMapper(Grid griddata)
            :base(griddata)
        {
            // calculate tranformation matrix
            List<IntPoint> srcRect = new List<IntPoint>();
            srcRect.Add(new IntPoint(0, 0));
            srcRect.Add(new IntPoint(srcWidth - 1, 0));
            srcRect.Add(new IntPoint(srcWidth - 1, srcHeight - 1));
            srcRect.Add(new IntPoint(0, srcHeight - 1));

            _matrix = this.MapQuadToQuad(destinationQuadrilateral, srcRect);
        }

        public override Point FromPresentation(Point p)
        {
            double factor = matrix[2, 0] * x + matrix[2, 1] * y + matrix[2, 2];
            double srcX = (matrix[0, 0] * x + matrix[0, 1] * y + matrix[0, 2]) / factor;
            double srcY = (matrix[1, 0] * x + matrix[1, 1] * y + matrix[1, 2]) / factor;

            if ((srcX >= 0) && (srcY >= 0) && (srcX < srcWidth) && (srcY < srcHeight))
            {
                // get pointer to the pixel in the source image
                p = baseSrc + (int)srcY * srcStride + (int)srcX * pixelSize;
                // copy pixel's values
                for (int i = 0; i < pixelSize; i++, ptr++, p++)
                {
                    *ptr = *p;
                }
            }
            else
            {
                // skip the pixel
                ptr += pixelSize;
            }
        }
    }
}
