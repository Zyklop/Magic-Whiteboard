using System;

namespace HSR.PresWriter.Common.Containers
{
    /// <summary>
    /// Container for a time dependent capture</summary>
    public abstract class Frame : IComparable
    {
        /// <summary>
        /// Frame Index</summary>
        public int Number { get; protected set; }

        /// <summary>
        /// Time when the frame was captured or when the frame was ready.
        /// This depends on how you initialized the frame. If you didn't
        /// Initialize the frame with a timestamp, the value is set to
        /// the constructors ending time.</summary>
        public long Timestamp { get; protected set; }

        /// <summary>
        /// Initializing a frame.
        /// If no timestamp is given, we set one as soon the frame is ready</summary>
        /// <param name="number">
        /// Frame Index</param>
        /// <param name="timestamp">
        /// Time when the object was captured in milliseconds</param>
        protected Frame(int number, long timestamp = 0)
        {
            Number = number;
            Timestamp = timestamp != 0 ? timestamp : CurrentMillis.Millis;
        }

        public int CompareTo(object obj)
        {
            var right = obj as Frame;
            return Number.CompareTo(right.Number);
        }
    }
}
