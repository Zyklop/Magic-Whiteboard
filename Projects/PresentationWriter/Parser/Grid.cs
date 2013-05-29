using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Windows;
using Point = System.Drawing.Point;
using HSR.PresWriter.Extensions;

namespace HSR.PresWriter.PenTracking
{
    public class Grid
    {
        private SortedDictionary<int, SortedDictionary<int, List<Point>>> _calibratorData; //2d Dictionary of imag-coordinates
        public Quad CameraQuad;
        private bool _needed = true;
        private Point _topLeft;
        private Point _topRight;
        private Point _bottomLeft;
        private Point _bottomRight;
        private SortedDictionary<int, SortedDictionary<int, Point>> _refPoints; //2d dictionary of screen Coordinates

        /// <summary>
        /// Creating a mapping grid
        /// </summary>
        /// <param name="width">width of the camera resolution</param>
        /// <param name="height">height of the camea resolution</param>
        public Grid(int width, int height)
        {
            CameraQuad = new Quad()
            {
                TopLeft = new AForge.Point(0, 0),
                TopRight = new AForge.Point(width, 0),
                BottomLeft = new AForge.Point(0, height),
                BottomRight = new AForge.Point(width, height),
            };
            _calibratorData = new SortedDictionary<int, SortedDictionary<int, List<Point>>>();
            _refPoints = new SortedDictionary<int, SortedDictionary<int, Point>>();
        }

        /// <summary>
        /// Maximum width of the matching area
        /// </summary>
        public int Width { get
        {
            return (int) (new List<double>
                {
                    TopRight.X - TopLeft.X, 
                    BottomRight.X - BottomLeft.X, 
                    TopRight.X - BottomLeft.X, 
                    BottomRight.X - TopLeft.X
                }).Max();
        } }

        /// <summary>
        /// Maximum height of the matching area
        /// </summary>
        public int Height
        {
            get
            {
                return (int)(new List<double>
                {
                    BottomRight.Y - TopLeft.Y, 
                    BottomRight.Y - TopRight.Y, 
                    BottomLeft.Y - TopRight.Y, 
                    BottomLeft.Y - TopLeft.Y
                }).Max();
            }
        }

        /// <summary>
        /// Borders of the PCs screenresolution
        /// <remarks>Just to increase the performance of some calculations</remarks>
        /// </summary>
        public Rectangle ScreenSize { get; set; }

        /// <summary>
        /// More data needed for Calibration
        /// </summary>
        public bool DataNeeded { get { return _needed; } }

        /// <summary>
        /// A good area for calibration
        /// </summary>
        /// <returns></returns>
        public Rectangle Needed()
        {
            return new Rectangle();
        }

        /// <summary>
        /// Top left corner
        /// </summary>
        public Point TopLeft
        {
            get { return _topLeft; }
            set { 
                _topLeft = value;
            }
        }

        /// <summary>
        /// Top right corner
        /// </summary>
        public Point TopRight
        {
            get { return _topRight; }
            set
            {
                _topRight = value;
            }
        }

        /// <summary>
        /// Bottom left corner
        /// </summary>
        public Point BottomLeft
        {
            get { return _bottomLeft; }
            set
            {
                _bottomLeft = value;
            }
        }

        /// <summary>
        /// Bottom left corner
        /// </summary>
        public Point BottomRight
        {
            get { return _bottomRight; }
            set
            {
                _bottomRight = value;
            }
        }

        /// <summary>
        /// Checking if a camerapixel is inside of the grid
        /// </summary>
        /// <param name="p">Point to br checked</param>
        /// <returns>true, if inside</returns>
        public bool Contains(Point p)
        {
            var b1 = new BarycentricCoordinate(p, TopLeft, TopRight, BottomLeft);
            if (b1.IsInside)
                return true;
            var b2 = new BarycentricCoordinate(p, TopRight, BottomLeft, BottomRight);
            return b2.IsInside;
        }

        /// <summary>
        /// Checks if the point is in the Grid
        /// </summary>
        /// <param name="centerOfGravity"></param>
        /// <returns></returns>
        public bool Contains(AForge.Point centerOfGravity)
        {
            return Contains(new Point((int)centerOfGravity.X, (int)centerOfGravity.Y));
        }

        /// <summary>
        /// Reset temporary calibration data
        /// </summary>
        public void Reset()
        {
            _calibratorData = new SortedDictionary<int, SortedDictionary<int, List<Point>>>();
        }

        /// <summary>
        /// Add a point from calibration
        /// </summary>
        /// <param name="screenX">Beamer coordinate</param>
        /// <param name="screenY">Beamer coordinate</param>
        /// <param name="imgX">Webcam coordinate</param>
        /// <param name="imgY"></param>
        public void AddPoint(int screenX, int screenY, int imgX, int imgY)
        {
            AddCalibratorPoint(imgX, imgY, new Point(screenX, screenY));
            AddRefPoints(screenX, screenY, new Point(imgX, imgY));
#if DEBUG
            using (var fs = new StreamWriter(new FileStream(@"C:\Temp\aforge\points.csv", FileMode.Append, FileAccess.Write)))
            {
                var s = screenX + "," + screenY + "," + imgX + "," + imgY + ",";
                fs.WriteLine(s);
                fs.Flush();
            }
#endif
        }

        /// <summary>
        /// Adding a point to the dictionary
        /// </summary>
        /// <param name="imgX"></param>
        /// <param name="imgY"></param>
        /// <param name="screen"></param>
        private void AddCalibratorPoint(int imgX, int imgY, Point screen)
        {
            if (!_calibratorData.ContainsKey(imgX))
                _calibratorData.Add(imgX, new SortedDictionary<int, List<Point>>());
            if (!_calibratorData[imgX].ContainsKey(imgY))
                _calibratorData[imgX].Add(imgY, new List<Point>{screen});
            else
                _calibratorData[imgX][imgY].Add(screen);
        }

        /// <summary>
        /// Add a point from calibration
        /// </summary>
        /// <param name="screen">Beamer coordinate</param>
        /// <param name="img">Webcam coordinate</param>
        public void AddPoint(Point screen, Point img)
        {
#if DEBUG
            using (var fs = new StreamWriter(new FileStream(@"C:\Temp\aforge\points.csv", FileMode.Append, FileAccess.Write)))
            {
                var s = screen.X + "," + screen.Y + "," + img.X + "," + img.Y + ",";
                fs.WriteLine(s);
                fs.Flush();
            }
#endif
            AddCalibratorPoint(img.X,img.Y,screen);
            AddRefPoints(screen.X, screen.Y, new Point(img.X, img.Y));
        }

        /// <summary>
        /// Adding the point to the reference dictionary
        /// </summary>
        /// <param name="screenX"></param>
        /// <param name="screenY"></param>
        /// <param name="p"></param>
        private void AddRefPoints(int screenX, int screenY, Point p)
        {
            if (_refPoints.ContainsKey(screenX))
            {
                if (_refPoints[screenX].ContainsKey(screenY))
                    _refPoints[screenX][screenY] = p;
                else
                    _refPoints[screenX].Add(screenY, p);
            }
            else
            {
                _refPoints.Add(screenX, new SortedDictionary<int, Point>());
                _refPoints[screenX].Add(screenY, p);
            }
        }

        /// <summary>
        /// returns an added reference pount
        /// </summary>
        /// <remarks>Not intended for big radii. It will be incorrect then</remarks>
        /// <param name="screenX"></param>
        /// <param name="screenY"></param>
        /// <param name="radius">radius to look</param>
        /// <returns>Image coordinates</returns>
        public Point GetRefPoint(int screenX, int screenY, int radius)
        {
            if (!_refPoints.ContainsKey(screenX))
            {
                for (int i = screenX - radius; i < screenX + radius; i++)
                {
                    if (_refPoints.ContainsKey(i))
                    {
                        if (!_refPoints[i].ContainsKey(screenY))
                        {
                            for (int j = screenY - radius; j < screenY + radius; j++)
                            {
                                if (_refPoints[i].ContainsKey(j))
                                    return _refPoints[i][j];
                            }
                        }
                        return _refPoints[i][screenY];
                    }
                }
                throw new ArgumentException("Key is not valid");
            }
            if (!_refPoints[screenX].ContainsKey(screenY))
            {
                for (int j = screenY - radius; j < screenY + radius; j++)
                {
                    if (_refPoints[screenX].ContainsKey(j))
                        return _refPoints[screenX][j];
                }
                throw new ArgumentException("Key is not valid");
            }
            return _refPoints[screenX][screenY];
        }

        /// <summary>
        /// Picking the n nearest points
        /// </summary>
        /// <param name="x">target x coordinate</param>
        /// <param name="y">target y coordinate</param>
        /// <param name="desired">number of neighbours</param>
        /// <returns>ordered by distance</returns>
        internal List<PointMapping> FindNearest(int x, int y, int desired)
        {
            //var cols = PickNearest(_calibratorData, x, desired);
            var cols = _calibratorData.PickNearest(x, desired);
            var p = new List<PointMapping>();
            foreach (var col in cols)
            {
                var tmp = col.Value.PickNearest(y, desired);
                foreach (var range in tmp)
                {
                    p.Add(new PointMapping{Image = new Point(col.Key, range.Key), Screen = Average(range.Value)});
                }
            }
            p = p.OrderBy(p1 => DistanceTo(p1.Screen,new Point(x,y))).ToList();
            if (desired >= p.Count)
                return p;
            return p.GetRange(0,desired);
        }

        /// <summary>
        /// Distance between points
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        private double DistanceTo(Point p1, Point p2)
        {
            var diff = new Point(p1.X - p2.X, p1.Y - p2.Y);
            return Math.Sqrt(diff.X * diff.X + diff.Y * diff.Y);
        }

        /// <summary>
        /// Average position
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        private Point Average(List<Point> points)
        {
            if (points.Count == 0)
                throw new ArgumentException("List is empty");
            if (points.Count == 1)
                return points.First();
            var res = new Point(points.Sum(x => x.X), points.Sum(x => x.Y));
            res.X = (int)Math.Round(res.X * 1.0 / points.Count);
            res.Y = (int)Math.Round(res.Y * 1.0 / points.Count);
            return res;
        }

        public Quad PresentationQuad
        {
            get
            {
                return new Quad()
                {
                    TopLeft = new AForge.Point(TopLeft.X, TopLeft.Y),
                    TopRight = new AForge.Point(TopRight.X, TopRight.Y),
                    BottomLeft = new AForge.Point(BottomLeft.X, BottomLeft.Y),
                    BottomRight = new AForge.Point(BottomRight.X, BottomRight.Y)
                };
            }
        }

        public Quad BeamerQuad
        {
            get
            {
                return new Quad()
                {
                    TopLeft = new AForge.Point(ScreenSize.X, ScreenSize.Y),
                    TopRight = new AForge.Point(ScreenSize.X + ScreenSize.Width, ScreenSize.Y),
                    BottomLeft = new AForge.Point(ScreenSize.X, ScreenSize.Y + ScreenSize.Height),
                    BottomRight = new AForge.Point(ScreenSize.X + ScreenSize.Width, ScreenSize.Y + ScreenSize.Height),
                };
            }
        }
    }
}

namespace HSR.PresWriter.Extensions
{
    public static class SortedDictionaryNeighboursExtension
    {

        /// <summary>
        /// Picks the nearest Values from the SortedDictionary
        /// </summary>
        /// <typeparam name="T">Type of the value</typeparam>
        /// <param name="src"></param>
        /// <param name="target">target key</param>
        /// <param name="count">number of desired values</param>
        /// <returns>selected range</returns>
        public static SortedDictionary<int, T> PickNearest<T>(this SortedDictionary<int, T> src, int target, int count)
        {
            var lower = new Queue<int>(src.Keys.Where(x => x <= target));
            var upper = new Queue<int>(src.Keys.Where(x => x > target).Reverse());
            var res = new SortedDictionary<int, T>();
            while (res.Count < count && (lower.Count > 0 || upper.Count > 0))
            {
                if (lower.Count > 0)
                {
                    if (upper.Count > 0)
                    {
                        if (Math.Abs(lower.Peek() - target) < Math.Abs(upper.Peek() - target))
                            res.Add(lower.Peek(), src[lower.Dequeue()]);
                        else
                            res.Add(upper.Peek(), src[upper.Dequeue()]);
                    }
                    else
                        res.Add(lower.Peek(), src[lower.Dequeue()]);
                }
                else
                    res.Add(upper.Peek(), src[upper.Dequeue()]);
            }
            return res;
        }
    }

}
