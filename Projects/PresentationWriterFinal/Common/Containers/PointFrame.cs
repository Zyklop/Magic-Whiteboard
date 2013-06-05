using System.Drawing;

namespace PresWriter.Common.Containers
{
    public class PointFrame : Frame
    {
        /// <summary>
        /// Linked Point</summary>
        public Point Point { get; protected set; }

        /// <summary>
        /// Initializing a frame.
        /// If no timestamp is given, we set one as soon the frame is ready</summary>
        /// <param name="number">
        /// Frame Index</param>
        /// <param name="point"></param>
        /// <param name="timestamp">
        /// Time when the object was captured in milliseconds</param>
        public PointFrame(int number, Point point, long timestamp = 0)
            : base(number, timestamp)
        {
            Point = point;
        }

        /// <summary>
        /// Return a repositioned version of the PointFrame
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public PointFrame ApplyRebase(Point p)
        {
            return new PointFrame(Number, p, Timestamp);
        }

        /// <summary>
        /// Set a new Position for this Pointframe
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public void ApplyRebaseInPlace(Point p)
        {
            Point = p;
        }
    }
}
