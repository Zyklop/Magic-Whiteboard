using AForge.Imaging;
using AForge.Imaging.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSR.PresWriter.PenTracking.Strategies
{
    public class LightBulbStrategy : FilterStrategy
    {
        public LightBulbStrategy()
        {
            DifferenceFilter = new Difference();
            GrayFilter       = new Grayscale(.1, .1, .1);
            ThresholdFilter  = new Threshold(35);
            BlobCounter      = new BlobCounter();
            BlobCounter.MinWidth = 3;
            BlobCounter.MinHeight = 3;
        }
    }
}
