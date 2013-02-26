using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSR.PresentationWriter.DataSources
{
    public interface ICamera : IDisposable
    {
        void Start();
        void Stop();
        Frame GetLastFrame();
    }
}
