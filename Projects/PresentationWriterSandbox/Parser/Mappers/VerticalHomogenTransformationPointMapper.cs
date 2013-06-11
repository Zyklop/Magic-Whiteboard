using System;
using System.Collections.Generic;
using AForge;
using HSR.PresWriter.Extensions;

namespace HSR.PresWriter.PenTracking.Mappers
{
    // the code of in this class is partly copied from AForge HomogenTransformation
    public class VerticalHomogenTransformationPointMapper : AbstractPointMapper
    {
        private SortedDictionary<float, SortedDictionary<float, Point>> _mapping;

        public VerticalHomogenTransformationPointMapper(Grid griddata)
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
            double kTop;
            double bTop;
            double kLeft;
            double bBottom;
            double kBottom;

            // top edge
            if (sourceQuadrilateral[1].X == sourceQuadrilateral[0].X)
            {
                kTop = 0;
                bTop = sourceQuadrilateral[1].X;
            }
            else
            {
                kTop = (double) (sourceQuadrilateral[1].Y - sourceQuadrilateral[0].Y)/
                        (sourceQuadrilateral[1].X - sourceQuadrilateral[0].X);
                bTop = sourceQuadrilateral[0].Y - kTop*sourceQuadrilateral[0].X;
            }

            // bottom edge
            if (sourceQuadrilateral[2].X == sourceQuadrilateral[3].X)
            {
                kBottom = 0;
                bBottom = sourceQuadrilateral[2].X;
            }
            else
            {
                kBottom = (double) (sourceQuadrilateral[2].Y - sourceQuadrilateral[3].Y)/
                           (sourceQuadrilateral[2].X - sourceQuadrilateral[3].X);
                bBottom = sourceQuadrilateral[3].Y - kBottom*sourceQuadrilateral[3].X;
            }

            // left edge
            if (sourceQuadrilateral[3].X == sourceQuadrilateral[0].X)
            {
                kLeft = 0;
            }
            else
            {
                kLeft = (double) (sourceQuadrilateral[3].Y - sourceQuadrilateral[0].Y)/
                         (sourceQuadrilateral[3].X - sourceQuadrilateral[0].X);
            }

            // some precalculated values
            double topFactor = (double)(sourceQuadrilateral[1].X - sourceQuadrilateral[0].X) / dstWidth;
            double bottomFactor = (double)(sourceQuadrilateral[3].X - sourceQuadrilateral[2].X) / dstWidth;

            var srcX0 = sourceQuadrilateral[0].X;
            var srcX1 = sourceQuadrilateral[1].X;

            // for each line
            for (int x = 0; x < dstWidth; x++)
            {
                // find corresponding Y on the left edge of the quadrilateral
                double yVerticTop = topFactor*x + srcX0;
                // find corresponding X on the left edge of the quadrilateral
                double xVerticTop = (kTop == 0) ? bTop : (yVerticTop - bTop)/kLeft;

                // find corresponding Y on the right edge of the quadrilateral
                double yVerticBottom = bottomFactor*x + srcX1;
                // find corresponding X on the left edge of the quadrilateral
                double xVerticBottom = (kBottom == 0) ? bBottom : (yVerticBottom - bBottom)/kBottom;

                // find equation of the line joining points on the left and right edges
                double kVertic, bVertic;

                if (xVerticTop == xVerticBottom)
                {
                    kVertic = 0;
                    bVertic = xVerticBottom;
                }
                else
                {
                    kVertic = (yVerticBottom - yVerticTop)/(xVerticBottom - xVerticTop);
                    bVertic = yVerticTop - kVertic*xVerticTop;
                }

                double verticFactor = (xVerticBottom - xVerticTop)/dstWidth;

                for (int y = 0; y < dstHeight; y++)
                {
                    var xs = (float) (verticFactor*x + xVerticTop);
                    var ys = (float) (kVertic*xs + bVertic);

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
