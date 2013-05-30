using System;
using System.Collections.Generic;
using AForge;
using HSR.PresWriter.Extensions;

namespace HSR.PresWriter.PenTracking.Mappers
{
    public class HorizontalHomogenTransformationPointMapper : AbstractPointMapper
    {
        private SortedDictionary<float, SortedDictionary<float, Point>> _mapping;

        public HorizontalHomogenTransformationPointMapper(Grid griddata)
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

            _mapping = new SortedDictionary<float, SortedDictionary<float, Point>>();

            // find equations of four quadrilateral's edges ( f(x) = k*x + b )
            double bLeft;
            double kLeft;
            double bRight;
            double kRight;


            // left edge
            if (sourceQuadrilateral[3].X == sourceQuadrilateral[0].X)
            {
                kLeft = 0;
                bLeft = sourceQuadrilateral[3].X;
            }
            else
            {
                kLeft = (double) (sourceQuadrilateral[3].Y - sourceQuadrilateral[0].Y)/
                         (sourceQuadrilateral[3].X - sourceQuadrilateral[0].X);
                bLeft = (double) sourceQuadrilateral[0].Y - kLeft*sourceQuadrilateral[0].X;
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
            double leftFactor = (double)(sourceQuadrilateral[3].Y - sourceQuadrilateral[0].Y) / dstHeight;
            double rightFactor = (double)(sourceQuadrilateral[2].Y - sourceQuadrilateral[1].Y) / dstHeight;

            var srcY0 = sourceQuadrilateral[0].Y;
            var srcY1 = sourceQuadrilateral[1].Y;

            // for each line
            for (int y = 0; y < dstHeight; y++)
            {
                // find corresponding Y on the left edge of the quadrilateral
                double yHorizLeft = leftFactor*y + srcY0;
                // find corresponding X on the left edge of the quadrilateral
                double xHorizLeft = (kLeft == 0) ? bLeft : (yHorizLeft - bLeft)/kLeft;

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
                    float xs = (float) (horizFactor*x + xHorizLeft);
                    float ys = (float) (kHoriz*xs + bHoriz);

                    if (true || (xs >= 0) && (ys >= 0) && (xs < srcWidth) && (ys < srcHeight))
                    {
                        if (!_mapping.ContainsKey(xs))
                            _mapping.Add(xs, new SortedDictionary<float, Point>());
                        if(!_mapping[xs].ContainsKey(ys))
                            _mapping[xs].Add(ys, new Point(x,y));
                        else
                            _mapping[xs][ys] = new Point(x,y);
                    }
                }
            }
        }

        public override Point FromPresentation(Point p)
        {
            return FindNearest(p);
        }


        /// <summary>
        /// Picking the n nearest points
        /// </summary>
        /// <param name="target">Target point</param>
        /// <returns>ordered by distance</returns>
        private Point FindNearest(Point target)
        {
            //var cols = PickNearest(_calibratorData, x, desired);
            var cols = _mapping.PickNearest(target.X, 20);
            var p = new Point(float.PositiveInfinity, float.PositiveInfinity);
            foreach (var col in cols)
            {
                var tmp = col.Value.PickNearest(target.Y, 40);
                foreach (var range in tmp)
                {
                    var pt = new Point(col.Key, range.Key);
                    if (target.DistanceTo(pt) <= target.DistanceTo(p))
                        p = pt;
                }
            }
            return p;
        }
    }
}
