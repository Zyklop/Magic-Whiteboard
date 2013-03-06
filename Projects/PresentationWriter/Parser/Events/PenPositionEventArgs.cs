using System;
using System.Windows;

namespace HSR.PresentationWriter.Parser.Events
{
    public class PenPositionEventArgs : EventArgs
    {
        private double _confidence;

        public Point Point { get; set; }
        public long Timestamp { get; set; }

        public double Confidance { 
            get 
            {
                return this._confidence;
            }
            set
            {
                if (value < 0 || value > 1)
                {
                    throw new ArgumentOutOfRangeException("Value range is [0..1].");
                }
                this._confidence = value;
            }
        }
    }
}
