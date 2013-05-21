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
    internal class IncrementalClusteringAForgeCalibrator : ICalibrator
    {
        private IPictureProvider _cc;
        private int _calibrationStep;
        private int _errors;
        private IVisualizerControl _vs;
        private const int CalibrationFrames = 6;
        private Difference diffFilter = new Difference();
        private const double SideWidthFactor = 0.04;
        private const double BgWidthFactor = 0.1;
        private SemaphoreSlim _sem;
        private Task _t;
        private Bitmap actImg;
        private int _sqrwidth;
        private int _bgwidth;

        public IncrementalClusteringAForgeCalibrator(IPictureProvider provider, IVisualizerControl visualizer)
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
            _sqrwidth = (int) Math.Round(_vs.Height * SideWidthFactor); // this may go wrong with exotic resolutions
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
                    var mf = new Median(3);
                    mf.ApplyInPlace(img);
                    img.Save(@"C:\temp\daforge\diff\img" + _calibrationStep + ".jpg", ImageFormat.Jpeg);
#if DEBUG
                    actImg = (Bitmap) img.Clone();
#endif
                    var blobCounter = new BlobCounter
                        {
                            ObjectsOrder = ObjectsOrder.YX,
                            MaxHeight = 20,
                            MinHeight = 5,
                            MaxWidth = 20,
                            MinWidth = 5,
                            BackgroundThreshold = Color.FromArgb(255, 15, 20, 20),
                            FilterBlobs = true,
                            CoupledSizeFiltering = false
                        };
                    blobCounter.ProcessImage(img);
                    if(ProcessBlobs(blobCounter))
                        _calibrationStep++;
#if DEBUG
                    actImg.Save(@"C:\temp\daforge\squares\img" + _calibrationStep + ".jpg", ImageFormat.Jpeg);
#endif
                    DrawMarkers();
                }
                else
                    switch (_calibrationStep)
                    {
                        case 2:
                            var bm = UnmanagedImage.FromManagedImage(e.Frame.Bitmap);
                            //diffFilter.OverlayImage.Save(@"C:\temp\daforge\diff\src" + _errors + ".jpg", ImageFormat.Jpeg);
                            bm = diffFilter.Apply(bm);
                            var gf = new Median(3);
                            gf.ApplyInPlace(bm);
                            var cf = new ColorFiltering(new IntRange(10, 255), new IntRange(20, 255),
                                                        new IntRange(20, 255));
                            cf.ApplyInPlace(bm);
                            var blobCounter = new BlobCounter
                                {
                                    ObjectsOrder = ObjectsOrder.Size,
                                    MinHeight = 200,
                                    MinWidth = 300,
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
                                    Grid.TopLeft.X < Grid.BottomRight.X &&
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
                                    DrawMarkers();
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

        private void DrawMarkers()
        {
            DrawBackground();
            var parts = Math.Pow(2, _calibrationStep - 2);
            var w = _vs.Width / parts;
            var h = _vs.Height / parts;
            for (int i = 0; i <= parts; i++)
            {
                for (int j = 0; j <= parts; j++)
                {
                    if (i % 2 == 1)
                    {
                        if (j % 2 == 1)
                            //center of a rectangle
                            DrawInverted((int) (i*w - _sqrwidth/2),(int) (j*h - _sqrwidth/2),_sqrwidth,_sqrwidth);
                        else
                        {
                            // middle of a horizontal outline
                            // check for top edge
                            if (j == 0)
                                DrawInverted((int)(i * w - _sqrwidth / 2), 0, _sqrwidth, _sqrwidth);
                            // check for bottom
                            else if (j == (int)parts)
                                DrawInverted((int)(i * w - _sqrwidth / 2), _vs.Height - _sqrwidth, _sqrwidth, _sqrwidth);
                            else
                                DrawInverted((int)(i * w - _sqrwidth / 2), (int)(j * h - _sqrwidth / 2), _sqrwidth, _sqrwidth);
                        }
                    }
                    else if (j % 2 == 1)
                    {
                        // middle of a horizontal outline
                        // check for left edge
                        if (i == 0)
                            DrawInverted(0, (int)(j * h - _sqrwidth / 2), _sqrwidth, _sqrwidth);
                        // check for right edge
                        else if (i == (int) parts)
                            DrawInverted(_vs.Width - _sqrwidth, (int) (j*h - _sqrwidth/2), _sqrwidth, _sqrwidth);
                        else
                            DrawInverted((int)(i * w - _sqrwidth / 2), (int)(j * h - _sqrwidth / 2), _sqrwidth, _sqrwidth);
                    }
                }
            }
            _vs.Draw();
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
                    if (((y/_bgwidth)%2 == 0 && (x/_bgwidth)%2 == 1 || (y/_bgwidth)%2 == 1 && (x/_bgwidth)%2 == 0))
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
        private bool ProcessBlobs(BlobCounter bc)
        {
            var blobs = new List<Blob>(bc.GetObjectsInformation());
            blobs.RemoveAll(x => !Grid.Contains(x.CenterOfGravity));
            //if (blobs.Count < 4)
            //{
            //    Debug.WriteLine("too few blobs");
            //    return false;
            //}
            //if (blobs.Count > 5)
            //{
            //    Debug.WriteLine("too many blobs");
            //    return false;
            //}
            var refs = new Dictionary<AForge.Point, Point>();
            var parts = (int)Math.Pow(2, _calibrationStep - 2);
            var w = _vs.Width/parts;
            var h = _vs.Height/parts;
            Point? missing = null;
            for (int i = 0; i <= parts; i++)
            {
                for (int j = 0; j <= parts; j++)
                {
                    if (i%2 == 1)
                    {
                        if (j%2 == 1)
                        {
                            //center of a rectangle
                            var tl = Grid.GetRefPoint((i - 1) * w, (j - 1) * h,1);
                            var tr = Grid.GetRefPoint((i + 1) * w, (j - 1) * h,1);
                            var bl = Grid.GetRefPoint((i - 1) * w, (j + 1) * h,1);
                            var br = Grid.GetRefPoint((i + 1) * w, (j + 1) * h,1);
                            refs.Add(new AForge.Point((tl.X + tr.X + bl.X + br.X)/4.0f,(tl.Y+tr.Y+bl.Y+br.Y)/4.0f), new Point(i,j));
                        }
                        else
                        {
                            // middle of a horizontal outline
                            var l = Grid.GetRefPoint((i - 1) * w, (j) * h,1);
                            var r = Grid.GetRefPoint((i + 1) * w, (j) * h,1);
                            refs.Add(new AForge.Point((l.X + r.X) / 2.0f, (l.Y + r.Y) / 2.0f), new Point(i,j));
                        }
                    }
                    else if (j%2 == 1)
                    {
                        // middle of a horizontal outline
                        var t = Grid.GetRefPoint((i) * w, (j - 1) * h,1);
                        var b = Grid.GetRefPoint((i) * w, (j + 1) * h,1);
                        refs.Add(new AForge.Point((t.X + b.X) / 2.0f, (t.Y + b.Y) / 2.0f), new Point(i,j));
                    }
                }
            }
            var clusters = KMeansCluster.KMeansClustering(blobs.Select(x => x.CenterOfGravity).ToList(),
                                                          refs.Keys.ToList(), 5);
            foreach (var cluster in clusters)
            {
                if (cluster.Points.Count(refs.ContainsKey) > 1)
                {
                    Debug.WriteLine("Multiple Centers in a Cluster");
                    return false;
                }
                if (!cluster.Points.Any(refs.ContainsKey))
                {
                    Debug.WriteLine("No Center in a Cluster");
                    return false;
                }
                var center = cluster.Points.First(refs.ContainsKey);
                var pos = refs[center];
                if (cluster.Points.Count == 1)
                {
                    Debug.WriteLine("Point missing in cluster");
                    if (missing.HasValue)
                        return false;
                    missing = pos;
                    continue;
                }
                if (cluster.Points.Count > 2)
                    Debug.WriteLine("Multiple Points in a cluster");
                cluster.Points.Remove(center);
#if DEBUG
                foreach (var point in cluster.Points)
                {
                    using (var g = Graphics.FromImage(actImg))
                    {
                        g.DrawString(pos.X + "," + pos.Y, new Font(FontFamily.GenericSansSerif, 8.0f),
                                     new SolidBrush(Color.Red),
                                     point.X, point.Y);
                        g.Flush();
                        //Debug.WriteLine("wrote to image");
                    }
                }
#endif
                var p = cluster.Points.First();
                var xOff = 0.0f;
                var yOff = 0.0f;
                if (pos.X == 0)
                {
                    //left
                    xOff += _sqrwidth/2.0f;
                }
                else if (pos.X == parts)
                {
                    //right
                    xOff -= _sqrwidth / 2.0f;
                }
                if (pos.Y == 0)
                {
                    //top
                    yOff += _sqrwidth / 2.0f;
                }
                else if (pos.Y == parts)
                {
                    //bottom
                    yOff -= _sqrwidth / 2.0f;
                }
                p.Y -= xOff;
                p.X -= yOff;
                var ip = p.Round();
                Grid.AddPoint(pos.X * w, pos.Y * h, ip.X, ip.Y);
            }
            //foreach (var blob in blobs)
            //{
            //    var corners = PointsCloud.FindQuadrilateralCorners(bc.GetBlobsEdgePoints(blob));
            //    //var xPos = blob.Value.X;
            //    //var yPos = blob.Value.Y;
            //    if (corners.Count == 4)
            //    {
            //        RecursiveAForgeCalibrator.GridBlobs.InPlaceSort(corners);
            //        //Grid.AddPoint((int) (xPos*_sqrwidth + xOff), (int) (yPos*_sqrheight + yOff), corners[0].X,
            //        //              corners[0].Y);
            //        //Grid.AddPoint((int) ((xPos + 1)*_sqrwidth + xOff), (int) (yPos*_sqrheight + yOff),
            //        //              corners[1].X,
            //        //              corners[1].Y);
            //        //Grid.AddPoint((int) (xPos*_sqrwidth + xOff), (int) ((yPos + 1)*_sqrheight + yOff),
            //        //              corners[2].X,
            //        //              corners[2].Y);
            //        //Grid.AddPoint((int) ((xPos + 1)*_sqrwidth + xOff), (int) ((yPos + 1)*_sqrheight + yOff),
            //        //              corners[3].X,
            //        //              corners[3].Y);
            //    }
            //}
            return true;
        }

        public Grid Grid { get; set; }
        public Colorfilter ColorFilter { get; private set; }

        public event EventHandler CalibrationCompleted;
    }

   
}
