using HSR.PresWriter.Containers;
using System;

namespace HSR.PresWriter.IO
{
    public interface ICamera : IPictureProvider, IDisposable
    {
        void Start();
        void Stop();
        bool IsRunning { get; }
        VideoFrame GetLastFrame();
    }
}
