using HSR.PresWriter.Containers;
using HSR.PresWriter.IO.Events;
using HSR.PresWriter.PenTracking.Events;
using System;
using System.Drawing;
using System.Threading.Tasks;

namespace HSR.PresWriter.PenTracking
{
    internal interface IPenTracker
    {
        PointFrame GetLastFrame();
        Point GetPenPoint(long timestamp);
        event EventHandler<PenPositionEventArgs> PenFound;
        void Start();
        void Stop();
        void ProcessAsync(object sender, FrameReadyEventArgs frameReadyEventArgs);
    }
}
