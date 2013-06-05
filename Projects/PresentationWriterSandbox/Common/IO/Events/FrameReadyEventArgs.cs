using HSR.PresWriter.Containers;

namespace HSR.PresWriter.IO.Events
{
    public class FrameReadyEventArgs
    {
        public readonly VideoFrame Frame; // TODO why no prperty?
        public FrameReadyEventArgs(VideoFrame frame)
        {
            this.Frame = frame;
        }
    }
}
