using HSR.PresentationWriter.Parser.Events;
using HSR.PresentationWriter.Parser.Images;
using System;
using System.Drawing;
using HSR.PresentationWriter.DataSources;

namespace HSR.PresentationWriter.Parser
{
    public interface IPenTracker
    {
        void Feed(Frame frame);
        Nullable<Point> GetLastPenPosition();
        Nullable<Point> GetPenPosition(long timestamp);
        event EventHandler<PenPositionEventArgs> PenMoved;
        event EventHandler<PenPositionEventArgs> PenDetected;
        event EventHandler<PenPositionEventArgs> PenLost;
    }
}
