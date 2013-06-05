using System;
using HSR.PresWriter.IO;
using PresWriter.Common.Containers;
using PresWriter.Common.IO;

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
