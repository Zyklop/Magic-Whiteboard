using AForge.Imaging;
using AForge.Imaging.Filters;

namespace HSR.PresWriter.PenTracking.Strategies
{
    public class WhiteLedStrategy : FilterStrategy
    {
        public WhiteLedStrategy()
        {
            DifferenceFilter = new Difference();
            GrayFilter       = new Grayscale(1,1,1);
            ThresholdFilter  = new Threshold(30);
            BlobCounter      = new BlobCounter();
            BlobCounter.FilterBlobs = true;
            BlobCounter.MinWidth    = 3;
            BlobCounter.MinHeight   = 3;
            BlobCounter.MaxWidth    = 15;
            BlobCounter.MaxHeight   = 15;
        }
    }
}
