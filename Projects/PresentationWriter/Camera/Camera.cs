using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSR.PresentationWriter.DataSources
{
    public interface ICamera : IDisposable
    {
        void Start();
        void Stop();
        VideoFrame GetLastFrame();
        event EventHandler<FrameReadyEventArgs> FrameReady;
    }
}
