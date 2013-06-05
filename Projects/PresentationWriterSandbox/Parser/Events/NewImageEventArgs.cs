using System;
using System.Drawing;
using HSR.PresWriter.PenTracking.Images;

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
