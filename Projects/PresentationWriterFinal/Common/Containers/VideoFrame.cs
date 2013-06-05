using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSR.PresWriter.Containers
{
    public class VideoFrame : Frame
    {
        /// <summary>
        /// Linked Picture</summary>
        public Bitmap Bitmap { get; set; }

        /// <summary>
        /// Initializing a frame.
        /// If no timestamp is given, we set one as soon the frame is ready</summary>
        /// <param name="number">
        /// Frame Index</param>
        /// <param name="image">
        /// Linked Picture</param>
        /// <param name="timestamp">
        /// Time when the object was captured in milliseconds</param>
        public VideoFrame(int number, Bitmap image, long timestamp = 0)
            : base(number, timestamp)
        {
            this.Bitmap = image;
        }
    }
}
