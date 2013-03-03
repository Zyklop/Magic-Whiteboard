using System.Windows;
using HSR.PresentationWriter.Parser.Events;
using HSR.PresentationWriter.Parser.Images;

namespace HSR.PresentationWriter.Parser
{
    internal class PenTracker
    {
        public static PenPositionEventArgs GetPenPosition(ThreeChannelBitmap image)
        {
            return new PenPositionEventArgs{Confidance = 0, Point = new Point()};
        }
    }
}
