using System;
using Parser.Images;

namespace Parser.Events
{
    public class NewImageEventArgs:EventArgs
    {
        public ThreeChannelBitmap NewImage { get; set; } 
    }
}
