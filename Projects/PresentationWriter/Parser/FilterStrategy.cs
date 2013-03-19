using AForge.Imaging;
using AForge.Imaging.Filters;

namespace HSR.PresentationWriter.Parser
{
    public abstract class FilterStrategy
    {
        /// <summary>
        /// Used Filters for PenTracking
        /// </summary>
        public Difference  DifferenceFilter { get; set; }
        public Grayscale   GrayFilter       { get; set; }
        public Threshold   ThresholdFilter  { get; set; }
        public BlobCounter BlobCounter      { get; set; }
    }
}
