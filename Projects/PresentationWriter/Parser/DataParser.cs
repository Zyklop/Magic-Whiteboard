using System;
using HSR.PresWriter.PenTracking.Events;
using HSR.PresWriter.PenTracking.Strategies;
using HSR.PresWriter.IO.Cameras;
using System.IO;
using HSR.PresWriter.Containers;
using HSR.PresWriter.IO;
using HSR.PresWriter.IO.Events;
using System.Diagnostics;
using Visualizer;
using Point = System.Drawing.Point;

namespace HSR.PresWriter.PenTracking
{
    public class DataParser
    {
        private IPictureProvider _pictureProvider;
        private ICalibrator _calibrator; // TODO Interface anpassen an StartCalibration etc
        private IPenTracker _penTracker;
        private int _gridcheck=1000;

        public bool IsRunning { get; protected set; }

        /// <summary>
        /// Set up a parser gor th images
        /// Parsing all the data
        /// </summary>
        /// <param name="provider"></param>
        public DataParser(IPictureProvider provider, IVisualizerControl visualizer)
        {
            _pictureProvider = provider;

            // Initialize Calibration Tools
            _calibrator = new AForgeDiffCalibrator(_pictureProvider, visualizer);
            _calibrator.CalibrationCompleted += StartTracking; // begin pen tracking after calibration immediately

            // Initialize Pen Tracking Tools
            _penTracker = new AForgePenTracker(new RedLaserStrategy(), _pictureProvider);
            _penTracker.PenFound += PenFound;
        }

        private void StartTracking(object sender, EventArgs e)
        {
            //_pictureProvider.ShowConfigurationDialog();
            //_pictureProvider.FrameReady += _camera_FrameReady; // TODO siehe _camera_FrameReady
            _calibrator.Grid.Calculate();
            _penTracker.Start();
            //_calibrator.Grid.PredictFromCorners();
        }

        /// <summary>
        /// Starting calibration and start tracking the pen afterwards
        /// </summary>
        public void Start()
        {
            this.IsRunning = true;
            //var filesystemCamera = new FilesystemCamera(new DirectoryInfo(@"c:\temp\aforge\inp"));
            //filesystemCamera.Start();
            //_calibrator = new AForgeDiffCalibrator(_pictureProvider, TODO); // TODO
            _calibrator.CalibrationCompleted += StartTracking;
            _calibrator.Calibrate();
        }

        public void Stop()
        {
            //_pictureProvider.FrameReady -= _camera_FrameReady; // TODO siehe _camera_FrameReady
            _penTracker.Stop();
            this.IsRunning = false;
        }

        //private async void _camera_FrameReady(object sender, FrameReadyEventArgs e)
        //{
        //    // TODO dem PenTracker auch einen PictureProvider übergeben und diese Methode streichen!
        //    PointFrame p = await _penTracker.ProcessAsync(e.Frame);
        //    if (p != null)
        //    {
        //        Debug.WriteLine("Point: {0}, {1}", p.Point.X, p.Point.Y);
        //    }
        //}

        private void PenFound(object sender, PenPositionEventArgs e)
        {
            // TODO Loswerden!!! Why???
            Point point = _calibrator.Grid.GetPosition(e.Frame.Point.X, e.Frame.Point.Y);
            PointFrame frame = e.Frame.ApplyRebase(point);
            if (PenPositionChanged != null)
            {
                bool isInside = point.X < Int32.MaxValue &&
                    point.X > 0 && point.Y > 0 && point.Y < Int32.MaxValue;
                PenPositionChanged(this, new VirtualPenPositionEventArgs(frame, isInside));
            }
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
        /// Calibrator with the grid data
        /// </summary>
        internal PrimitiveCalibrator Calibrator { get; set; }

        public Grid CalibratorGrid { get { return _calibrator.Grid; } }

        /// <summary>
        /// The pen changed the position
        /// </summary>
        public event EventHandler<PenPositionEventArgs> PenPositionChanged;
    }
}
