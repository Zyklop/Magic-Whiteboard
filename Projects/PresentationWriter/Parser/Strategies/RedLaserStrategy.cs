﻿using AForge.Imaging;
using AForge.Imaging.Filters;

namespace HSR.PresentationWriter.Parser.Strategies
{
    public class RedLaserStrategy : FilterStrategy
    {
        public RedLaserStrategy()
        {
            DifferenceFilter = new Difference();
            GrayFilter       = new Grayscale(1, 0, 0);
            ThresholdFilter  = new Threshold(40);
            BlobCounter      = new BlobCounter();
        }
    }
}
