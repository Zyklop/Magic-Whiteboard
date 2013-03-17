using HSR.PresentationWriter.Parser.Events;
using HSR.PresentationWriter.Parser.Images;
using System;
using System.Drawing;
using HSR.PresentationWriter.DataSources;
using System.Threading.Tasks;

namespace HSR.PresentationWriter.Parser
{
    public interface IPenTracker
    {
        Task<PointFrame> ProcessAsync(VideoFrame frame);
        PointFrame GetLastFrame();
        Point GetPenPoint(long timestamp);
        event EventHandler<PenPositionEventArgs> PenMoved;
        event EventHandler<PenPositionEventArgs> PenDetected;
        event EventHandler<PenPositionEventArgs> PenLost;
    }
}
