using HSR.PresWriter.Containers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HSR.PresWriter.PenTracking
{
    public class PenFoundEventArgs:EventArgs
    {
        /// <summary>
        /// Location of the pen
        /// </summary>
        // why PointFrame? 
        // TODO use a common type!
        public PointFrame Frame { get; private set; }
        public bool IsOutOfOrder { get; private set; }

        public PenFoundEventArgs(PointFrame frame, bool isOutOfOrder = false)
        {
            IsOutOfOrder = isOutOfOrder;
            Frame = frame;
        }
    }
}
