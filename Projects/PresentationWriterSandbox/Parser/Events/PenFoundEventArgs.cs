using HSR.PresWriter.Containers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HSR.PresWriter.PenTracking
{
    public class PenFoundEventArgs:EventArgs
    {
        public PointFrame Frame { get; private set; }

        public PenFoundEventArgs(PointFrame frame)
        {
            Frame = frame;
        }
    }
}
