using System;
using PresWriter.Common.IO.Events;

namespace PresWriter.Common.IO
{
    public interface IPictureProvider
    {
        event EventHandler<FrameReadyEventArgs> FrameReady;
    }
}
