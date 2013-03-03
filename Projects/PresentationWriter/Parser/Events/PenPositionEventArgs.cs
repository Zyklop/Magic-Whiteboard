using System;
using System.Windows;

namespace HSR.PresentationWriter.Parser.Events
{
    public class PenPositionEventArgs:EventArgs
    {
        public Point Point { get; set; }
        public int Confidance { get; set; }
    }
}
