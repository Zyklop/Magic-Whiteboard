using System;
using HSR.PresWriter.PenTracking.Calibrators;
using HSR.PresWriter.PenTracking.Events;
using HSR.PresWriter.PenTracking.Mappers;
using HSR.PresWriter.PenTracking.Strategies;
using PresWriter.Common.Containers;
using PresWriter.Common.IO;
using Visualizer;
using Point = System.Drawing.Point;

namespace HSR.PresWriter.PenTracking
{
    public class DataParser
    {
        private ICalibrator _calibrator; // TODO Interface anpassen an StartCalibration etc
        private IPenTracker _penTracker;
        private Type _mapperType;
        private AbstractPointMapper _mapper;
        private int _gridcheck=1000;

        public bool IsRunning { get; protected set; }

        /// <summary>
        /// Set up a parser gor th images
        /// Parsing all the data
        /// </summary>
        /// <param name="provider">Provider of the Camera images</param>
        /// <param name="visualizer">The Output to write on</param>
        public DataParser(IPictureProvider provider, IVisualizerControl visualizer)
        {
            // Initialize Calibration Tools
            _calibrator = new SimpleAForgeCalibrator(provider, visualizer);
            _calibrator.CalibrationCompleted += StartTracking; // begin pen tracking after calibration immediately

            // Initialize Pen Tracking Tools
            _penTracker = new AForgePenTracker(new WhiteLedStrategy(), provider);
            _penTracker.PenFound += PenFound;
            _penTracker.NoPenFound += NoPenFound;

            _mapperType = typeof(BarycentricIntegralPointMapper);
        }

        private void NoPenFound(object sender, EventArgs e)
        {
            if (PenPositionChanged != null)
            {
                PenPositionChanged(this, new VirtualPenPositionEventArgs(null, false));
            }
        }

        /// <summary>
        /// Initialize the Parser with a non-standart calibrator or tracker
        /// </summary>
        /// <param name="calibrator">Custom calibrator</param>
        /// <param name="tracker">Custom PenTracker</param>
        public DataParser(ICalibrator calibrator, IPenTracker tracker, Type mapperType)
        {
            if(!(typeof(AbstractPointMapper).IsAssignableFrom(mapperType)))
                throw new ArgumentException(mapperType.FullName + " doesn't inherit form AbstractPointMapper");
            // Initialize Calibration Tools
            _calibrator = calibrator;
            _calibrator.CalibrationCompleted += StartTracking; // begin pen tracking after calibration immediately

            // Initialize Pen Tracking Tools
            _penTracker = tracker;
            _penTracker.PenFound += PenFound;
            _penTracker.NoPenFound += NoPenFound;

            _mapperType = mapperType;
        }

        /// <summary>
        /// Calibration is finished, starting PenTracker
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartTracking(object sender, EventArgs e)
        {
            //_pictureProvider.ShowConfigurationDialog();
            //_pictureProvider.FrameReady += _camera_FrameReady; // TODO siehe _camera_FrameReady
            //_calibrator.Grid.Calculate();
            var mCtor = _mapperType.GetConstructor(new Type[] { typeof(Grid) });
            _mapper = (AbstractPointMapper)mCtor.Invoke(new Grid[] { CalibratorGrid });
            //_mapper = (AbstractPointMapper)Activator.CreateInstance(_mapperType, 
            //    BindingFlags.CreateInstance, null, new Grid[] { CalibratorGrid });
            //if (_mapperType.Equals(typeof(LinearMapper))) // check for runtimeType
            //{
            //    ((LinearMapper)_mapper).Calculate();
            //}
            Console.WriteLine("Calbration completed");
            _penTracker.Start();
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

        /// <summary>
        /// Restart the Parser again, without recalibrating
        /// </summary>
        public void Restart()
        {
            if(CalibratorGrid.TopLeft.IsEmpty)
                throw new InvalidOperationException("Needs to be Calibrated first. Use Start()");
            _penTracker.Start();
        }

        /// <summary>
        /// Stopping the PenTracker
        /// <remarks>Cannot be used to stop the calibration, this have to be completed first</remarks>
        /// </summary>
        public void Stop()
        {
            //_pictureProvider.FrameReady -= _camera_FrameReady; // TODO siehe _camera_FrameReady
            _penTracker.Stop();
            this.IsRunning = false;
        }

        /// <summary>
        /// Start the Calibration again
        /// </summary>
        public void ReCalibrate()
        {
            // TODO not working, why?
            _penTracker.Stop();
            _calibrator.Calibrate();
            Console.WriteLine("Recalibrating");
        }

        /// <summary>
        /// Mapping the point
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PenFound(object sender, PenFoundEventArgs e)
        {
            Point point = _mapper.FromPresentation(e.Frame.Point.X, e.Frame.Point.Y);
            PointFrame frame = e.Frame.ApplyRebase(point);
            if (PenPositionChanged != null)
            {
                PenPositionChanged(this, new VirtualPenPositionEventArgs(frame, CalibratorGrid.Contains(point)));
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
        internal ICalibrator Calibrator { get; set; }

        /// <summary>
        /// TODO
        /// </summary>
        public Grid CalibratorGrid { get { return _calibrator.Grid; } }

        /// <summary>
        /// The pen changed the position
        /// </summary>
        public event EventHandler<PenPositionEventArgs> PenPositionChanged;
    }
}
