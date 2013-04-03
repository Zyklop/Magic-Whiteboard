using HSR.PresWriter.Containers;
using HSR.PresWriter.PenTracking.Events;
using System;
using System.Drawing;
using System.Threading.Tasks;

namespace HSR.PresWriter.PenTracking
{
    internal interface IPenTracker
    {
        Task<PointFrame> ProcessAsync(VideoFrame frame);
        PointFrame GetLastFrame();
        Point GetPenPoint(long timestamp);
        event EventHandler<InternalPenPositionEventArgs> PenFound;
    }
}
