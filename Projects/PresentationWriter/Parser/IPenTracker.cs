using HSR.PresWriter.Containers;
using HSR.PresWriter.IO.Events;
using HSR.PresWriter.PenTracking.Events;
using System;
using System.Drawing;
using System.Threading.Tasks;

namespace HSR.PresWriter.PenTracking
{
    public interface IPenTracker
    {
        PointFrame GetLastFrame();
        Point GetPenPoint(long timestamp);
        event EventHandler<PenFoundEventArgs> PenFound;
        event EventHandler<EventArgs> NoPenFound;
        void Start();
        void Stop();
    }
}
