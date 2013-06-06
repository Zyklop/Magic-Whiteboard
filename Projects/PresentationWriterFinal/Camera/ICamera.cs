using System;
using HSR.PresWriter.Common.Containers;
using HSR.PresWriter.Common.IO;

namespace HSR.PresWriter.DataSources
{
    public interface ICamera : IPictureProvider, IDisposable
    {
        void Start();
        void Stop();
        bool IsRunning { get; }
        VideoFrame GetLastFrame();
    }
}
