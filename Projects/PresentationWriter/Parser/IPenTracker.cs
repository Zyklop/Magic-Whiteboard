using HSR.PresWriter.Containers;
using HSR.PresWriter.PenTracking.Events;
using System;
using System.Drawing;
using System.Threading.Tasks;

namespace HSR.PresWriter.PenTracking
{
    public interface IPenTracker
    {
        Task<PointFrame> ProcessAsync(VideoFrame frame);
        PointFrame GetLastFrame();
        Point GetPenPoint(long timestamp);
        event EventHandler<PenPositionEventArgs> PenFound;
    }
}
