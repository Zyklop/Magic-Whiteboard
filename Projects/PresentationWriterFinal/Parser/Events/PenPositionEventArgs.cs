using System;
using PresWriter.Common.Containers;

namespace HSR.PresWriter.PenTracking.Events
{
    public class PenPositionEventArgs : EventArgs
    {
        /// <summary>
        /// Location of the pen
        /// </summary>
        public PointFrame Frame { get; private set; }

        public bool IsOutOfOrder { get; private set; }

        public PenPositionEventArgs(PointFrame frame, bool isOutOfOrder = false)
        {
            IsOutOfOrder = isOutOfOrder;
            Frame = frame;
        }
    }

    public class VirtualPenPositionEventArgs : PenPositionEventArgs
    {
        //private double _confidence;
        public bool IsInside { get; protected set; }

        /// <summary>
        /// Probability of accuracy
        /// </summary>
        /// <value>Has to be between 0 and 1</value>
        //public double Confidance
        //{
        //    get
        //    {
        //        return this._confidence;
        //    }
        //    set
        //    {
        //        if (value < 0 || value > 1)
        //        {
        //            throw new ArgumentOutOfRangeException("Value range is [0..1].");
        //        }
        //        this._confidence = value;
        //    }
        //}

        public VirtualPenPositionEventArgs(PointFrame frame, bool inside)
            :base(frame)
        {
            IsInside = inside;
        }
    }
}
