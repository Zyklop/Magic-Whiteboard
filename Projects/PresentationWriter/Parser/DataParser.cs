using System;
using System.Windows;
using HSR.PresWriter.PenTracking.Events;
using HSR.PresWriter.PenTracking.Strategies;
using HSR.PresWriter.IO.Cameras;
using System.IO;

namespace HSR.PresWriter.PenTracking
{
    public class DataParser
    {
        private ICalibrator _calibrator;
        private IPenTracker _penTracker;
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
            _calibrator = new AForgeCalibrator(new FilesystemCamera(new DirectoryInfo(@"c:\temp\"))); // TODO
            _calibrator.CalibrationCompleted += StartTracking;
        }

        private void StartTracking(object sender, EventArgs e)
        {
            _calibrator.Grid.PredictFromCorners();
            _penTracker = new AForgePenTracker(new RedLaserStrategy());
            _penTracker.PenFound += PenFound;
        }

        private void PenFound(object sender, InternalPenPositionEventArgs e)
        {
            PenPositionChanged(this, new PenPositionEventArgs(_calibrator.Grid.GetPosition(e.Frame.Point.X, e.Frame.Point.Y), e.Confidance));
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
        internal PrimitiveCalibrator Calibrator { get; set; }

        /// <summary>
        /// The pen changed the position
        /// </summary>
        public event EventHandler<PenPositionEventArgs> PenPositionChanged;
    }
}
