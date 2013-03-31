using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSR.PresWriter.Containers
{
    public class PointFrame : Frame
    {
        /// <summary>
        /// Linked Point</summary>
        public readonly Point Point;

        /// <summary>
        /// Initializing a frame.
        /// If no timestamp is given, we set one as soon the frame is ready</summary>
        /// <param name="number">
        /// Frame Index</param>
        /// <param name="image">
        /// Linked Point</param>
        /// <param name="timestamp">
        /// Time when the object was captured in milliseconds</param>
        public PointFrame(int number, Point point, long timestamp = 0)
            : base(number, timestamp)
        {
            this.Point = point;
        }
    }
}
