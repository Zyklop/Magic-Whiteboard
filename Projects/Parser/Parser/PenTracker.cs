using Parser.Events;
using Parser.Images;

namespace Parser
{
    internal class PenTracker
    {
        public static PenPositionEventArgs GetPenPosition(ThreeChannelBitmap image)
        {
            return new PenPositionEventArgs{Confidance = 0, Point = new Point()};
        }
    }
}
