using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser
{
    internal class Calibrator
    {
        private CameraConnector _cc;

        public Calibrator(CameraConnector cc)
        {
            _cc = cc;
            Calibrate();
            CalibrateColors();
        }

        public void CalibrateColors()
        {
            throw new NotImplementedException();
        }

        public void Calibrate()
        {
            throw new NotImplementedException();
        }

        public int CheckCalibration()
        {
            
        }

        public Grid Grid { get; private set; }

        public Colorfilter ColorFilter { get; private set; }
    }
}
