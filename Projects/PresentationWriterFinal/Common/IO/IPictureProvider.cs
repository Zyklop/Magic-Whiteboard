using System;
using PresWriter.Common.IO.Events;

namespace HSR.PresWriter.Common.IO
{
    public interface IPictureProvider
    {
        event EventHandler<FrameReadyEventArgs> FrameReady;
    }
}
