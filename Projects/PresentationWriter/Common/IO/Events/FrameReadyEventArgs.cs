using HSR.PresWriter.Containers;

namespace HSR.PresWriter.IO.Events
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
