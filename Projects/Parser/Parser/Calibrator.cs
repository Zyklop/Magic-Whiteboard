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
        }

        public void Calibrate()
        {
        }

        public int CheckCalibration()
        {
            return 0;
        }

        public Grid Grid { get; private set; }

        public Colorfilter ColorFilter { get; private set; }
    }
}
