using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSR.PresWriter.PenTracking
{
    public struct PointMapping
    {
        public Point Screen { get; set; }
        public Point Image { get; set; }
    }
}
