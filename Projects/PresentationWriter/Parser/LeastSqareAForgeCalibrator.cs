using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using AForge;
using AForge.Imaging;
using AForge.Imaging.Filters;
using AForge.Math.Geometry;
using HSR.PresWriter.IO;
using HSR.PresWriter.IO.Events;
using Visualizer;
using Point = System.Drawing.Point;

namespace HSR.PresWriter.PenTracking
{
    internal class LeastSqareAForgeCalibrator : ICalibrator
    {
        private IPictureProvider _cc;
        private int _calibrationStep;
        private int _errors;
        private IVisualizerControl _vs;
        private const int CalibrationFrames = 6;
        private Difference diffFilter = new Difference();
        private const double SideWidthFactor = 0.05;
        private const double BgWidthFactor = 0.1;
        private SemaphoreSlim _sem;
        private Task _t;
        private Bitmap actImg;
        private int _sqrwidth;
        private int _bgwidth;

        public LeastSqareAForgeCalibrator(IPictureProvider provider, IVisualizerControl visualizer)
        {
            _cc = provider;
            _vs = visualizer;
            //var thread = new Thread(() => _vs = new CalibratorWindow());
            //thread.SetApartmentState(ApartmentState.STA);
            //thread.Start();
            //thread.Join();
        }

        // TODO Calibrate Funktionen aufräumen
        public void StartCalibration()
        {
            Calibrate();
            CalibrateColors();
        }


        public void CalibrateColors()
        {
        }

        public void Calibrate()
        {
            _calibrationStep = 0;
            _errors = 0;
            _sqrwidth = (int) Math.Round(_vs.Height * SideWidthFactor);
            _bgwidth = (int) Math.Round(_vs.Height*BgWidthFactor);
            _sem = new SemaphoreSlim(1, 1);
            _cc.FrameReady += BaseCalibration;
        }

        public int CheckCalibration()
        {
            return 0;
        }

        private void BaseCalibration(object sender, FrameReadyEventArgs e)
        {
            if (_t != null && _t.Exception != null)
            {
                throw _t.Exception;
            }
            if (_sem.CurrentCount >= 1)//_t.Status != TaskStatus.Running)
            {
                _sem.Wait();
                Task.Factory.StartNew(() => _t = CalibThread(e));
            }
        }


        private async Task CalibThread(FrameReadyEventArgs e)
        {
            Debug.WriteLine("Calibrating " + _calibrationStep + " ");
            e.Frame.Bitmap.Save(@"C:\temp\daforge\src\img" + (_calibrationStep<10?"0":"") + _calibrationStep + "-" + _errors + ".jpg", ImageFormat.Jpeg);
            if (_errors > 100)
            {
                //calibration not possible
                return;
            }
            if (_calibrationStep == CalibrationFrames)
            {
                _cc.FrameReady -= BaseCalibration;
                //Grid.Calculate();
                _vs.Close();
                CalibrationCompleted(this, new EventArgs());
            }
            else
            {
                if (_calibrationStep > 2)
                {
                    // analyse diffimage
                    _vs.Clear();
                    var img = diffFilter.Apply(e.Frame.Bitmap);
                    img.Save(@"C:\temp\daforge\diff\img" + _calibrationStep + ".jpg", ImageFormat.Jpeg);
#if DEBUG
                    actImg = (Bitmap) img.Clone();
#endif
                    var mf = new Median {Size = 5};
                    mf.ApplyInPlace(img);
                    var blobCounter = new BlobCounter
                        {
                            ObjectsOrder = ObjectsOrder.YX,
                            MaxHeight = 25,
                            MinHeight = 10,
                            MaxWidth = 25,
                            MinWidth = 10,
                            FilterBlobs = true,
                            CoupledSizeFiltering = false
                        };
                    ProcessBlobs(blobCounter);
#if DEBUG
                    actImg.Save(@"C:\temp\daforge\squares\img" + _calibrationStep + ".jpg", ImageFormat.Jpeg);
#endif
                    _calibrationStep++;
                }
                else
                    switch (_calibrationStep)
                    {
                        case 2:
                            var bm = UnmanagedImage.FromManagedImage(e.Frame.Bitmap);
                            //diffFilter.OverlayImage.Save(@"C:\temp\daforge\diff\src" + _errors + ".jpg", ImageFormat.Jpeg);
                            bm = diffFilter.Apply(bm);
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
                            bm.ToManagedImage()
                              .Save(@"C:\temp\daforge\diff\img" + _calibrationStep + "-" + _errors + ".jpg",
                                    ImageFormat.Jpeg);
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
                                RecursiveAForgeCalibrator.GridBlobs.InPlaceSort(corners);
                                Grid.TopLeft = new Point(corners[0].X, corners[0].Y);
                                Grid.TopRight = new Point(corners[1].X, corners[1].Y);
                                Grid.BottomLeft = new Point(corners[2].X, corners[2].Y);
                                Grid.BottomRight = new Point(corners[3].X, corners[3].Y);
                                if (Grid.TopLeft.X > 10 && Grid.TopRight.X < _vs.Width - 5 && Grid.BottomLeft.X > 5 &&
                                    Grid.BottomRight.X < _vs.Width - 5 && Grid.TopLeft.Y > 10 && Grid.TopRight.Y > 5 &&
                                    Grid.BottomLeft.Y < _vs.Height - 5 && Grid.BottomRight.Y < _vs.Height - 5 &&
                                    Grid.TopLeft.X < Grid.BottomRight.X && blobs[i - 1].Area > 60000 &&
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
                                    Grid.PredictFromCorners();
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
                            }
                            break;
                        case 1:
                            diffFilter.OverlayImage = e.Frame.Bitmap;
                            //diffFilter.OverlayImage.Save(@"C:\temp\daforge\diff\srcf" + _errors + ".jpg", ImageFormat.Jpeg);
                            //_vs.AddRect(0, 0, (int) _vs.Width, (int) _vs.Height, Color.FromArgb(255, 255, 255, 255));
                            _vs.Clear();
                            DrawInverted(0,0,_vs.Width,_vs.Height);
                            _vs.Draw();
                            _calibrationStep++;
                            break;
                        case 0:
                            Grid = new Grid(e.Frame.Bitmap.Width, e.Frame.Bitmap.Height);
                            Grid.ScreenSize = new Rectangle(0, 0, _vs.Width, _vs.Height);
                            var thread = new Thread(() =>
                                {
                                    _vs.Show();
                                    _vs.AddRect(0, 0, _vs.Width, _vs.Height, Color.FromArgb(255, 0, 0, 0));
                                });
                            thread.SetApartmentState(ApartmentState.STA);
                            thread.Start();
                            thread.Join();
                            DrawBackground();
                            _vs.Draw();
                            _calibrationStep++;
                            break;
                    }
            }
            Debug.WriteLine("Releasing");
            await Task.Delay(500);
            _sem.Release();
        }

        private void DrawInverted(int left, int top, int width, int height)
        {
            for (int y = top; y < top + height;)
            {
                var h = _bgwidth - y%_bgwidth;
                if (y + h - top > height)
                    h -= y + h - top - height;
                for (int x = left; x < left+width;)
                {
                    var w = _bgwidth - x%_bgwidth;
                    if (x + w - left > width)
                        w -= x + w - left - width;
                    if (!(y%2 == 0 && x%2 == 0 || y%2 == 1 && x%2 == 1))
                        _vs.AddRect(x, y, w, h, Color.White);
                    else
                        _vs.AddRect(x, y, w, h, Color.Black);
                    x += w;
                }
                y += h;
            }
        }

        private void DrawBackground()
        {
            _vs.Clear();
            for (int y = 0; y < _vs.Height; y+=_bgwidth)
            {
                for (int x = 0; x < _vs.Width; x+=_bgwidth)
                {
                    if ((y/_bgwidth)%2 == 0 && (x/_bgwidth)%2 == 0 || (y/_bgwidth)%2 == 1 && (x/_bgwidth)%2 == 1)
                    {
                        _vs.AddRect(x, y, _bgwidth, _bgwidth, Color.FromArgb(255, 255, 255, 255));
                    }
                }
            }
        }

        /// <summary>
        /// calculate the blob's position based on the grids position
        /// </summary>
        /// <param name="b"></param>
        /// <param name="offset"></param>
        private void ProcessBlobs(BlobCounter b)
        {
            var blobs = new List<Blob>(b.GetObjectsInformation());
            
            foreach (var blob in blobs)
            {
#if DEBUG
                using (var g = Graphics.FromImage(actImg))
                {
                    //g.DrawString(blob.Value.X + "," + blob.Value.Y, new Font(FontFamily.GenericSansSerif, 8.0f),
                    //             new SolidBrush(Color.White),
                    //             blob.Key.CenterOfGravity.X, blob.Key.CenterOfGravity.Y);
                    g.Flush();
                    //Debug.WriteLine("wrote to image");
                }
#endif
                var corners = PointsCloud.FindQuadrilateralCorners(b.GetBlobsEdgePoints(blob));
                //var xPos = blob.Value.X;
                //var yPos = blob.Value.Y;
                if (corners.Count == 4)
                {
                    RecursiveAForgeCalibrator.GridBlobs.InPlaceSort(corners);
                    //Grid.AddPoint((int) (xPos*_sqrwidth + xOff), (int) (yPos*_sqrheight + yOff), corners[0].X,
                    //              corners[0].Y);
                    //Grid.AddPoint((int) ((xPos + 1)*_sqrwidth + xOff), (int) (yPos*_sqrheight + yOff),
                    //              corners[1].X,
                    //              corners[1].Y);
                    //Grid.AddPoint((int) (xPos*_sqrwidth + xOff), (int) ((yPos + 1)*_sqrheight + yOff),
                    //              corners[2].X,
                    //              corners[2].Y);
                    //Grid.AddPoint((int) ((xPos + 1)*_sqrwidth + xOff), (int) ((yPos + 1)*_sqrheight + yOff),
                    //              corners[3].X,
                    //              corners[3].Y);
                }
            }
        }

        public Grid Grid { get; set; }
        public Colorfilter ColorFilter { get; private set; }

        public event EventHandler CalibrationCompleted;
    }
}
