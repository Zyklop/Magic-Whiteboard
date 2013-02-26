using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSR.PresentationWriter.DataSources
{
    class DirectShowCamera : ICamera
    {
        public void Start()
        {

        }

        public void Stop()
        {

        }

        public Frame GetLastFrame()
        {
            return null;
        }

        public void Dispose()
        {
            this.Stop();
        }
    }
}
