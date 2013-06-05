using PresWriter.Common.Containers;

namespace PresWriter.Common.IO.Events
{
    public class FrameReadyEventArgs
    {
        public VideoFrame Frame { get; private set; };
        public FrameReadyEventArgs(VideoFrame frame)
        {
            Frame = frame;
        }
    }
}
