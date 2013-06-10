using System;
using HSR.PresWriter.Common.Containers;
using HSR.PresWriter.Common.IO;

namespace HSR.PresWriter.DataSources
{
    public interface ICamera : IPictureProvider, IDisposable
    {
        /// <summary>
        /// Start capturing images
        /// </summary>
        void Start();

        /// <summary>
        /// Stop the capture
        /// </summary>
        void Stop();

        /// <summary>
        /// Check, if the Camera is enabled
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// Get the last Image
        /// </summary>
        /// <returns>The last VideoFrame or null if no frame is avaliable</returns>
        VideoFrame GetLastFrame();
    }
}
