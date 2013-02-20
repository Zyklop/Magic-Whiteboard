using System;
using Parser.Images;

namespace Parser.Events
{
    internal class NewImageEventArgs:EventArgs
    {
        public ThreeChannelBitmap NewImage { get; set; } 
    }
}
