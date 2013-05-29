using AForge;

namespace HSR.PresWriter.PenTracking
{
    public struct Quad
    {
        public Point TopLeft { get; set; }
        public Point TopRight { get; set; }
        public Point BottomLeft { get; set; }
        public Point BottomRight { get; set; }

        /// <summary>
        /// Create a Square with sides of length 1.
        /// </summary>
        public static Quad UnitSquare()
        {
            return new Quad()
            {
                TopLeft = new Point(0, 0),
                TopRight = new Point(1, 0),
                BottomLeft = new Point(0, 1),
                BottomRight = new Point(1, 1)
            };
        }
    }
}
