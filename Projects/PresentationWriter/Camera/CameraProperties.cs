namespace HSR.PresentationWriter.DataSources
{
    /// <summary>
    /// Used to initialize an AVICAP-Webcam</summary>
    public abstract class CameraProperties
    {
        /// <summary>
        /// Width of the AVICAP virtual window</summary>
        public int Width;

        /// <summary>
        /// Height of the AVICAP virtual window</summary>
        public int Height;

    }

    /// <summary>
    /// Used by Camera if nothing else is supplied.</summary>
    public class StandardCameraProperties : CameraProperties
    {
        public StandardCameraProperties()
        {
            Width = 320;
            Height = 240;
        }
    }
}
