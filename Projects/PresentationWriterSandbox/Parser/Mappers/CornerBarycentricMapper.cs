using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AForge;

namespace HSR.PresWriter.PenTracking.Mappers
{
    public class CornerBarycentricMapper: AbstractPointMapper
    {
        public CornerBarycentricMapper(Grid grid) : base(grid)
        {
        }

        public override Point FromPresentation(Point p)
        {
            var x = (int)Math.Round(p.X);
            var y = (int)Math.Round(p.Y);
            var b1 = new BarycentricCoordinate(new System.Drawing.Point(x, y), Grid.TopLeft, Grid.TopRight, 
                Grid.BottomLeft);
            var p1 = b1.GetCartesianCoordinates(new System.Drawing.Point(Grid.ScreenSize.Left, Grid.ScreenSize.Top),
                new System.Drawing.Point(Grid.ScreenSize.Right, Grid.ScreenSize.Top), 
                new System.Drawing.Point(Grid.ScreenSize.Left, Grid.ScreenSize.Bottom));
            //return p1;
            //if (b1.IsInside)
            //    return p1;
            var b2 = new BarycentricCoordinate(new System.Drawing.Point(x, y), Grid.TopLeft, 
                Grid.BottomLeft, Grid.BottomRight);
            var p2 = b2.GetCartesianCoordinates(new System.Drawing.Point(Grid.ScreenSize.Left, Grid.ScreenSize.Top),
                new System.Drawing.Point(Grid.ScreenSize.Left, Grid.ScreenSize.Bottom),
                new System.Drawing.Point(Grid.ScreenSize.Right, Grid.ScreenSize.Bottom));
            var b3 = new BarycentricCoordinate(new System.Drawing.Point(x, y), Grid.TopRight, 
                Grid.BottomLeft, Grid.BottomRight);
            var p3 = b3.GetCartesianCoordinates(new System.Drawing.Point(Grid.ScreenSize.Right, Grid.ScreenSize.Top),
                new System.Drawing.Point(Grid.ScreenSize.Left, Grid.ScreenSize.Bottom),
                new System.Drawing.Point(Grid.ScreenSize.Right, Grid.ScreenSize.Bottom));
            var b4 = new BarycentricCoordinate(new System.Drawing.Point(x, y), Grid.TopLeft, Grid.TopRight, Grid.BottomRight);
            var p4 = b4.GetCartesianCoordinates(new System.Drawing.Point(Grid.ScreenSize.Left, Grid.ScreenSize.Top),
                new System.Drawing.Point(Grid.ScreenSize.Right, Grid.ScreenSize.Top), 
                new System.Drawing.Point(Grid.ScreenSize.Right, Grid.ScreenSize.Bottom));
            //return p2;
            //if (b2.IsInside)
            //{
            //    return p2;
            //}
            if (!(b1.IsInside || b3.IsInside || b2.IsInside || b4.IsInside))
                return new Point(-1.0f, -1.0f);
            return new Point((p1.X + p2.X + p3.X + p4.X) / 4.0f, (p1.Y + p2.Y + p3.Y + p4.Y) / 4.0f);
            //if (!b1.IsInside && !b2.IsInside)
            //    return new Point();
            //return new Point((int) Math.Round((p1.X + p2.X)/2.0), (int) Math.Round((p1.Y + p2.Y)/2.0));
        }
    }
}
