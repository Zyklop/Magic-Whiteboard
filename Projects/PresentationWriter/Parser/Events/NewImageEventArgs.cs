using System;
using HSR.PresentationWriter.Parser.Images;

namespace HSR.PresentationWriter.Parser.Events
{
    public class NewImageEventArgs:EventArgs
    {
        public ThreeChannelBitmap NewImage { get; set; } 
    }
}
