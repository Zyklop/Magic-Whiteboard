using System;
using System.Drawing;
using HSR.PresentationWriter.Parser.Images;

namespace HSR.PresentationWriter.Parser.Events
{
    public class NewImageEventArgs:EventArgs
    {
        public Bitmap NewImage { get; set; } 
    }
}
