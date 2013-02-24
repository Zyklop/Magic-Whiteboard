using System;
using System.Drawing;

namespace HSR.PresentationWriter.DataSources
{
    /// <summary>
    /// Container for a captured webcam image</summary>
    public class Frame
    {
        /// <summary>
        /// Frame Index</summary>
        public readonly int Number;

        /// <summary>
        /// Linked Picture</summary>
        public readonly Image Image;

        /// <summary>
        /// Time when the picture was taken or when the frame was ready.
        /// This depends on how you initialized the frame. If you didn't
        /// Initialize the frame with a timestamp, the value is set to
        /// the constructors ending time.</summary>
        public readonly long Timestamp;

        /// <summary>
        /// Initializing a frame.
        /// If no timestamp is given, we set one as soon the frame is ready</summary>
        /// <param name="number">
        /// Frame Index</param>
        /// <param name="image">
        /// Linked Picture</param>
        /// <param name="timestamp">
        /// Time when the picture was taken in milliseconds</param>
        public Frame(int number, Image image, long timestamp = 0)
        {
            this.Number = number;
            this.Image = image;
            this.Timestamp = timestamp != 0 ? timestamp : CurrentMillis.Millis;
        }
    }
}
