using System;
using HSR.PresWriter.Common.Containers;

namespace HSR.PresWriter.PenTracking.Events
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
