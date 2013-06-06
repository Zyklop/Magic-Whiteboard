using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Linq;
using System.Threading.Tasks;
using AForge;
using AForge.Imaging;
using AForge.Imaging.Filters;
using AForge.Math.Geometry;
using HSR.PresWriter.Common.IO;
using PresWriter.Common.IO;
using PresWriter.Common.IO.Events;
using Visualizer;
using Point = System.Drawing.Point;

namespace HSR.PresWriter.PenTracking.Calibrators
{

    public interface ICalibrator
    {
        /// <summary>
        /// Calibrate the grid
        /// </summary>
        void Calibrate();

        /// <summary>
        /// Check if the grid needs to be recalibrated
        /// Check if something moved
        /// </summary>
        /// <returns>
        /// 0: ok
        /// 1: grid calibration needed
        /// 2: color calibration needed
        /// 3: both needed
        /// </returns>
        int CheckCalibration();

        /// <summary>
        /// Data for the matching
        /// </summary>
        Grid Grid { get; }

        /// <summary>
        /// Data from color calibration
        /// </summary>
        //Colorfilter ColorFilter { get; }

        event EventHandler CalibrationCompleted;
    }
    /// <summary>
    /// this calibrator just identifies the corners of the grid
    /// </summary>
    public class SimpleAForgeCalibrator : ICalibrator
    {
        private readonly IPictureProvider _cc;
        private int _calibrationStep;
        private int _errors;
        private readonly IVisualizerControl _vs;
        private const int CalibrationFrames = 3; //must be n^2+3
        private readonly Difference _diffFilter = new Difference();
        private SemaphoreSlim _sem;
        private Task _t;
        private double _sqrheight;
        private double _sqrwidth;


        public SimpleAForgeCalibrator(IPictureProvider provider, IVisualizerControl visualizer)
        {
            _cc = provider;
            _vs = visualizer;
            MaxErrorCount = 20;
            Rowcount = 20;
            Columncount = 15;
            MillisecondsDelay = 500;
        }
        public void StartCalibration()
        {
            Calibrate();
        }

        public void Calibrate()
        {
            _calibrationStep = 0;
            _errors = 0;
            _sqrwidth = _vs.Width / (double) Rowcount;
            _sqrheight = _vs.Height / (double) Columncount;
            _sem = new SemaphoreSlim(1, 1);
            _cc.FrameReady += BaseCalibration; // TODO
        }

        public int VisibleColumns { get { return Columncount - ((Math.Floor((_calibrationStep - 3) / Math.Sqrt(CalibrationFrames - 3)) == 0.0) ? 0 : 1); } }

        public int VisibleRows { get { return Rowcount - ((((_calibrationStep - 3)%(int) Math.Sqrt(CalibrationFrames - 3)) == 0) ? 0 : 1); } }

        public int CheckCalibration()
        {
            return 0;
        }

        private void BaseCalibration(object sender, FrameReadyEventArgs e)
        {
            if (_t != null && _t.Exception != null)
            {
                Logger.Log(_t.Exception);
            }
            if (_sem.CurrentCount >= 1)
            {
                _sem.Wait();
                Task.Factory.StartNew(() => _t = CalibThread(e));
            }
        }


        private async Task CalibThread(FrameReadyEventArgs e)
        {
            try
            {
                if (_errors > MaxErrorCount)
                {
                    _cc.FrameReady -= BaseCalibration;
                    _vs.Close();
                    Console.WriteLine("Calibration impossible");
                }
                if (_calibrationStep == CalibrationFrames)
                {
                    _cc.FrameReady -= BaseCalibration;
                    _vs.Close();
                    Console.WriteLine("Calibration complete");
                    CalibrationCompleted(this, new EventArgs());
                }
                else
                    switch (_calibrationStep)
                    {
                            // get the corners from the difference image
                        case 2:
                            var bm = UnmanagedImage.FromManagedImage(e.Frame.Bitmap);
                            bm = _diffFilter.Apply(bm);
                            var gf = new GaussianBlur(9.0, 3);
                            gf.ApplyInPlace(bm);
                            var cf = new ColorFiltering(new IntRange(10, 255), new IntRange(20, 255),
                                                        new IntRange(20, 255));
                            cf.ApplyInPlace(bm);
                            var blobCounter = new BlobCounter
                                {
                                    ObjectsOrder = ObjectsOrder.Size,
                                    BackgroundThreshold = Color.FromArgb(255, 15, 20, 20),
                                    FilterBlobs = true
                                };
                            blobCounter.ProcessImage(bm);
                            var blobs = blobCounter.GetObjectsInformation();
                            if (blobs.Any())
                            {
                                int i = 0;
                                List<IntPoint> corners;
                                do
                                {
                                    corners =
                                        PointsCloud.FindQuadrilateralCorners(blobCounter.GetBlobsEdgePoints(blobs[i++]));
                                } while (corners.Count != 4);
                                InPlaceSort(corners);
                                Grid.TopLeft = new Point(corners[0].X, corners[0].Y);
                                Grid.TopRight = new Point(corners[1].X, corners[1].Y);
                                Grid.BottomLeft = new Point(corners[2].X, corners[2].Y);
                                Grid.BottomRight = new Point(corners[3].X, corners[3].Y);
                                if (Grid.TopLeft.X > 10 && Grid.TopRight.X < _vs.Width - 5 && Grid.BottomLeft.X > 5 &&
                                    Grid.BottomRight.X < _vs.Width - 5 && Grid.TopLeft.Y > 10 && Grid.TopRight.Y > 5 &&
                                    Grid.BottomLeft.Y < _vs.Height - 5 && Grid.BottomRight.Y < _vs.Height - 5 &&
                                    Grid.TopLeft.X < Grid.BottomRight.X && //blobs[i - 1].Area > 60000 &&
                                    Grid.BottomLeft.X < Grid.TopRight.X && Grid.BottomLeft.X < Grid.BottomRight.X &&
                                    Grid.TopLeft.Y < Grid.BottomLeft.Y && Grid.TopLeft.Y < Grid.BottomRight.Y &&
                                    Grid.TopRight.Y < Grid.BottomLeft.Y && Grid.TopRight.Y < Grid.BottomRight.Y)
                                {
                                    _calibrationStep++;
                                    _vs.Clear();
                                    Grid.AddPoint(new Point(), new Point(corners[0].X, corners[0].Y));
                                    Grid.AddPoint(new Point(0, _vs.Height), new Point(corners[1].X, corners[1].Y));
                                    Grid.AddPoint(new Point(_vs.Width, 0), new Point(corners[2].X, corners[2].Y));
                                    Grid.AddPoint(new Point(_vs.Width, _vs.Height),
                                                  new Point(corners[3].X, corners[3].Y));
                                }
                                else
                                {
                                    _calibrationStep = 0;
                                    _errors++;
                                }
                            }
                            else
                            {
                                _calibrationStep = 0;
                                _errors++;
                                _vs.Draw();
                            }
                            break;
                        case 1: // draw second image, store the first
                            _diffFilter.OverlayImage = e.Frame.Bitmap;
                            _vs.Clear();
                            for (int y = 0; y < Columncount; y++)
                            {
                                for (int x = 0; x < Rowcount; x++)
                                {
                                    if (!(y%2 == 0 && x%2 == 0 || y%2 == 1 && x%2 == 1))
                                    {
                                        _vs.AddRect((int) (x*_sqrwidth), (int) (y*_sqrheight),
                                                    (int) _sqrwidth, (int) _sqrheight,
                                                    Color.FromArgb(255, 255, 255, 255));
                                    }
                                }
                            }
                            _vs.Draw();
                            _calibrationStep++;
                            break;
                            //draw the first image
                        case 0:
                            Grid = new Grid(e.Frame.Bitmap.Width, e.Frame.Bitmap.Height)
                                {
                                    ScreenSize = new Rectangle(0, 0, _vs.Width, _vs.Height)
                                };
                            var thread = new Thread(() =>
                                {
                                    _vs.Show();
                                    _vs.AddRect(0, 0, _vs.Width, _vs.Height, Color.FromArgb(255, 0, 0, 0));
                                });
                            thread.SetApartmentState(ApartmentState.STA);
                            thread.Start();
                            thread.Join();
                            for (int y = 0; y < Columncount; y++)
                            {
                                for (int x = 0; x < Rowcount; x++)
                                {
                                    if (y%2 == 0 && x%2 == 0 || y%2 == 1 && x%2 == 1)
                                    {
                                        _vs.AddRect((int) (x*_sqrwidth), (int) (y*_sqrheight),
                                                    (int) _sqrwidth, (int) _sqrheight,
                                                    Color.FromArgb(255, 255, 255, 255));
                                    }
                                }
                            }
                            _vs.Draw();
                            _calibrationStep++;
                            break;
                    }
                await Task.Delay(MillisecondsDelay);
            }
            finally
            {
                _sem.Release();
            }
        }

        private static void InPlaceSort(List<IntPoint> rect)
        {
            if (rect == null || rect.Count != 4)
                throw new ArgumentException("Not a rectancle");
            var tmp = new List<IntPoint>(rect);
            rect.Clear();
            var tl = tmp.First(x => x.Y == tmp.Min(y => y.Y));
            tmp.Remove(tl);
            rect.Add(tl);
            tl = tmp.First(x => x.Y == tmp.Min(y => y.Y));
            tmp.Remove(tl);
            rect.Add(tl);
            rect.Sort((x, y) => x.X.CompareTo(y.X));
            tmp = tmp.OrderBy(x => x.X).ToList();
            rect.Add(tmp[0]);
            rect.Add(tmp[1]);
        }

        public Grid Grid { get; private set; }

        public event EventHandler CalibrationCompleted;

        public int MaxErrorCount { get; set; }

        public int Rowcount { get; set; }

        public int Columncount { get; set; }

        public int MillisecondsDelay { get; set; }
    }
}
