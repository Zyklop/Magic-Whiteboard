using System;
using HSR.PresentationWriter.Parser.Images;

namespace HSR.PresentationWriter.Parser.Events
{
    internal class NewImageEventArgs:EventArgs
    {
        public ThreeChannelBitmap NewImage { get; set; } 
    }
}
