using HSR.PresWriter.Containers;
using System;
using System.Windows;

namespace HSR.PresWriter.PenTracking.Events
{
    public class PenPositionEventArgs : EventArgs
    {
        private double _confidence;

        /// <summary>
        /// Location of the pen
        /// </summary>
        public Point Point { get; private set; }

        public PenPositionEventArgs(Point frame, double confidence = 1)
        {
            Confidance = confidence;
            this.Point = frame;
        }

        /// <summary>
        /// Probability of accuracy
        /// </summary>
        /// <value>Has to be between 0 and 1</value>
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
