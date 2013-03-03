using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSR.PresentationWriter.DataSources
{
    public class FrameReadyEventArgs
    {
        public readonly Frame Frame;
        public FrameReadyEventArgs(Frame frame)
        {
            this.Frame = frame;
        }
    }
}
