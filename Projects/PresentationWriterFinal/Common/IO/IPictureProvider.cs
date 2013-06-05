using HSR.PresWriter.IO.Events;
using System;

namespace HSR.PresWriter.IO
{
    public interface IPictureProvider
    {
        event EventHandler<FrameReadyEventArgs> FrameReady;
    }
}
