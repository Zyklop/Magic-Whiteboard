using System;
using System.Windows;
using HSR.PresWriter.PenTracking.Events;

namespace HSR.PresWriter.PenTracking
{
    public class DataParser
    {
        private ICalibrator _calibrator;
        private int _gridcheck=1000;

        /// <summary>
        /// Set up a parser gor th images
        /// Parsing all the data
        /// </summary>
        /// <param name="camera"></param>
        public DataParser()
        {
            // TODO
        }

        /// <summary>
        /// Initializing and starting the calibration 
        /// </summary>
        private void Initialize()
        {
            _calibrator = new AForgeCalibrator(null); // TODO
        }

        /// <summary>
        /// Starting a calibration and start tracking the pen afterward
        /// </summary>
        public void Start()
        {
            Initialize();
            // TODO
        }

        private void NewImage(object sender, NewImageEventArgs e)
        {
            //if (PenPositionChanged != null) PenPositionChanged(this, PenTracker.GetPenPosition(e.NewImage));
            _gridcheck--;
            if (_gridcheck == 0) // calibration check needed
            {
                _gridcheck = 1000;
                switch (_calibrator.CheckCalibration())
                {
                    case 1: Calibrator.Calibrate();
                        break;
                    case 2: Calibrator.CalibrateColors();
                        break;
                }
            }
        }

        /// <summary>
        /// Stopp tracking the pen
        /// </summary>
        public void Stop()
        {
            // TODO
        }

        /// <summary>
        /// Dummy
        /// </summary>
        public Point Topl { get { return _calibrator.Grid.TopLeft; } }

        /// <summary>
        /// Calibrator with the grid data
        /// </summary>
        internal Calibrator Calibrator { get; set; }

        /// <summary>
        /// The pen changed the position
        /// </summary>
        public event EventHandler<PenPositionEventArgs> PenPositionChanged;
    }
}
