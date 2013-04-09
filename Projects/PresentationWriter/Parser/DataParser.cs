using System;
using System.Drawing;
using HSR.PresWriter.PenTracking.Events;
using HSR.PresWriter.PenTracking.Strategies;
using HSR.PresWriter.IO.Cameras;
using System.IO;
using HSR.PresWriter.Containers;
using HSR.PresWriter.IO;
using HSR.PresWriter.IO.Events;

namespace HSR.PresWriter.PenTracking
{
    public class DataParser
    {
        private IPictureProvider _pictureProvider;
        private AForgeCalibrator _calibrator; // TODO Interface anpassen an StartCalibration etc
        private IPenTracker _penTracker;
        private int _gridcheck=1000;

        public bool IsRunning { get; protected set; }

        /// <summary>
        /// Set up a parser gor th images
        /// Parsing all the data
        /// </summary>
        /// <param name="provider"></param>
        public DataParser(IPictureProvider provider)
        {
            _pictureProvider = provider;

            // Initialize Calibration Tools
            _calibrator = new AForgeCalibrator(_pictureProvider);
            _calibrator.CalibrationCompleted += StartTracking; // begin pen tracking after calibration immediately

            // Initialize Pen Tracking Tools
            _penTracker = new AForgePenTracker(new RedLaserStrategy());
            _penTracker.PenFound += PenFound;
        }

        private void StartTracking(object sender, EventArgs e)
        {
            //_pictureProvider.ShowConfigurationDialog();
            _calibrator.Grid.PredictFromCorners();
            _pictureProvider.FrameReady += _camera_FrameReady; // TODO siehe _camera_FrameReady
        }

        /// <summary>
        /// Starting calibration and start tracking the pen afterwards
        /// </summary>
        public void Start()
        {
            //var filesystemCamera = new FilesystemCamera(new DirectoryInfo(@"c:\temp\aforge\inp"));
            //filesystemCamera.Start();
            _calibrator = new AForgeCalibrator(_pictureProvider); // TODO
            _calibrator.CalibrationCompleted += StartTracking;
            this.IsRunning = true;
            _calibrator.StartCalibration();
        }

        public void Stop()
        {
            this.IsRunning = false;
            _pictureProvider.FrameReady -= _camera_FrameReady; // TODO siehe _camera_FrameReady
        }

        private async void _camera_FrameReady(object sender, FrameReadyEventArgs e)
        {
            // TODO dem PenTracker auch einen PictureProvider übergeben und diese Methode streichen!
            PointFrame p = await _penTracker.ProcessAsync(e.Frame);
        }

        private void PenFound(object sender, PenPositionEventArgs e)
        {
            // TODO Loswerden!!!
            System.Windows.Point p = _calibrator.Grid.GetPosition(e.Frame.Point.X, e.Frame.Point.Y);
            Point point = new Point((int)p.X, (int)p.Y);
            PointFrame frame = e.Frame.ApplyRebase(point);
            PenPositionChanged(this, new PenPositionEventArgs(frame, point.X < Int32.MaxValue && 
                point.X > 0 && point.Y > 0 && point.Y < Int32.MaxValue, e.Confidance));
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
