using System;
using System.Windows;
using HSR.PresentationWriter.Parser.Events;

namespace HSR.PresentationWriter.Parser
{
    public class DataParser
    {
        private readonly CameraConnector _cc;
        private Calibrator _calibrator;
        private int _gridcheck=1000;

        public DataParser(CameraConnector camera)
        {
            _cc = camera;
        }

        public void Initialize()
        {
            _calibrator = new Calibrator(_cc);
        }

        public void Start()
        {
            _cc.NewImage += NewImage;
        }

        private void NewImage(object sender, NewImageEventArgs e)
        {
            if (PenPositionChanged != null) PenPositionChanged(this, PenTracker.GetPenPosition(e.NewImage));
            _gridcheck--;
            if (_gridcheck == 0)
            {
                _gridcheck = 1000;
                switch (Calibrator.CheckCalibration())
                {
                    case 1: Calibrator.Calibrate();
                        break;
                    case 2: Calibrator.CalibrateColors();
                        break;
                }
            }
        }

        public void Stop()
        {
            _cc.NewImage -= NewImage;
        }

        public Point Topl { get { return _calibrator.Grid.TopLeft; } }

        internal Calibrator Calibrator { get; set; }

        public event EventHandler<PenPositionEventArgs> PenPositionChanged;
    }
}
