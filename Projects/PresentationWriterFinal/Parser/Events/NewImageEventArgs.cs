using System;
using System.Drawing;

namespace HSR.PresWriter.PenTracking.Events
{
    public class NewImageEventArgs:EventArgs
    {
        /// <summary>
        /// The new image
        /// </summary>
        public Bitmap NewImage { get; set; } 
    }
}
