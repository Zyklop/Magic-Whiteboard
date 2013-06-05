using System;
using System.Collections.Generic;
using SDPoint = System.Drawing.Point;
using APoint = AForge.Point;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSR.PresWriter.PenTracking
{
    public abstract class AbstractPointMapper
    {
        /// <summary>
        /// Grid holds the information for the mapping process
        /// </summary>
        public Grid Grid { get; protected set; }

        public AbstractPointMapper(Grid grid)
        {
            Grid = grid;
        }

        /// <summary>
        /// Overwrite this to map coordinates
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public abstract APoint FromPresentation(APoint p);

        /// <summary>
        /// Conversion from System.Drawing.Point to AForge and back
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public SDPoint FromPresentation(SDPoint p)
        {
            APoint po = FromPresentation(new APoint(p.X,p.Y));
            po.Round();
            return new SDPoint((int)po.X, (int)po.Y);
        }

        public SDPoint FromPresentation(int x, int y)
        {
            return FromPresentation(new SDPoint(x, y));
        }
    }
}
