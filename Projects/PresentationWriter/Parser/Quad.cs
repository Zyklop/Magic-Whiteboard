using AForge;

namespace HSR.PresWriter.PenTracking
{
    public struct Quad
    {
        public Point TopLeft { get; set; }
        public Point TopRight { get; set; }
        public Point BottomLeft { get; set; }
        public Point BottomRight { get; set; }
    }
}
