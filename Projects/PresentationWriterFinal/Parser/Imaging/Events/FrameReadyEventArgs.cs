using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSR.PresentationWriter.Parser.Imaging.Events
{
    public class FrameReadyEventArgs
    {
        public readonly VideoFrame Frame;
        public FrameReadyEventArgs(VideoFrame frame)
        {
            this.Frame = frame;
        }
    }
}
