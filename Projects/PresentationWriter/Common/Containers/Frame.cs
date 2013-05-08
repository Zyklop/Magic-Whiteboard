using System;
using System.Drawing;

namespace HSR.PresWriter.Containers
{
    /// <summary>
    /// Container for a time dependent capture</summary>
    public abstract class Frame : IComparable
    {
        /// <summary>
        /// Frame Index</summary>
        public readonly int Number;

        /// <summary>
        /// Time when the frame was captured or when the frame was ready.
        /// This depends on how you initialized the frame. If you didn't
        /// Initialize the frame with a timestamp, the value is set to
        /// the constructors ending time.</summary>
        public readonly long Timestamp;

        /// <summary>
        /// Initializing a frame.
        /// If no timestamp is given, we set one as soon the frame is ready</summary>
        /// <param name="number">
        /// Frame Index</param>
        /// <param name="timestamp">
        /// Time when the object was captured in milliseconds</param>
        public Frame(int number, long timestamp = 0)
        {
            this.Number = number;
            this.Timestamp = timestamp != 0 ? timestamp : CurrentMillis.Millis;
        }

        public int CompareTo(object obj)
        {
            Frame right = obj as Frame;
            return this.Number.CompareTo(right.Number);
        }
    }
}
