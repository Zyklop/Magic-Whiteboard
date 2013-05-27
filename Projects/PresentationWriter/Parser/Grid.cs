using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Windows;
using Point = System.Drawing.Point;

namespace HSR.PresWriter.PenTracking
{
    public class Grid
    {
        private SortedDictionary<int, SortedDictionary<int, List<Point>>> _calibratorData;
        //2d Dictionary of imag-coordinates

        public Quad CameraQuad;

        private Point[,] _mapData; //calculated Points
        private bool _needed = true;
        private Point _topLeft;
        private Point _topRight;
        private Point _bottomLeft;
        private Point _bottomRight;
        private SortedDictionary<int, SortedDictionary<int, Point>> _refPoints; //2d dictionary of screen Coordinates
        private int[] iterationNeighbours = { 10, 15, 19, 24, 30 };
        //private int[] iterationNeighbours = {4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30 };

    private const int NeighboursNeeded = 7;
#if DEBUG
        public int[] NeighbourUsedCount { get; set; }
#endif

        /// <summary>
        /// Creating a mapping grid
        /// </summary>
        /// <param name="width">width of the source image</param>
        /// <param name="height">height of the source image</param>
        public Grid(int width, int height)
        {
            CameraQuad = new Quad()
            {
                TopLeft = new AForge.Point(0, 0),
                TopRight = new AForge.Point(width, 0),
                BottomLeft = new AForge.Point(0, height),
                BottomRight = new AForge.Point(width, height),
            };

            _mapData = new Point[width,height];
            _calibratorData = new SortedDictionary<int, SortedDictionary<int, List<Point>>>();
            _refPoints = new SortedDictionary<int, SortedDictionary<int, Point>>();
#if DEBUG
            using (var fs = new StreamWriter(new FileStream(@"C:\Temp\aforge\points.csv", FileMode.Create, FileAccess.Write)))
            {
                fs.WriteLine("ScreenX,ScreenY,ImageX,ImageY");
                fs.Flush();
            }
            NeighbourUsedCount = new int[iterationNeighbours.Length];
#endif
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
                    // TODO dafuq?
                    BottomRight.Y - TopLeft.Y, 
                    BottomRight.Y - TopLeft.Y, 
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
        /// Calculate from collected data
        /// </summary>
        public void Calculate()
        {
            int xmax = (int) ScreenSize.Width;
            int ymax = (int) ScreenSize.Height;
            for (int i = 0; i < xmax; i++)
            {
                for (int j = 0; j < ymax; j++)
                {
                    var p = Interpolate(i, j);
                    AddCalibratorPoint(p.X, p.Y, new Point(i,j));
                    //if (_calibratorData.ContainsKey(p.X) && _calibratorData[p.X].ContainsKey(p.Y))
                    //    _calibratorData[p.X,p.Y].Add();
                    //else 
                    //    _calibratorData[p.X,p.Y] = new List<Point>{new Point(i,j)};
                }
            }
            SetMapData();
            Debug.WriteLine("Calculation complete");
            //_refPoints.Clear();
        }

        /// <summary>
        /// Interpolating Barycentric from the next Points
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private Point Interpolate(int x, int y)
        {
            var poss = new List<Point>();
            List<PointMapping> n;
            for (int i = 0; i < iterationNeighbours.Length && poss.Count < NeighboursNeeded; i++)
            {
                n = FindNearest(x, y, iterationNeighbours[i]);
                poss = GetCandidates(x, y, n);
#if DEBUG
                if (poss.Count >= NeighboursNeeded)
                {
                    NeighbourUsedCount[i]++;
                }
#endif
            }
            return Average(poss);
        }

        /// <summary>
        /// Predict from each possible triange, returning each candidate
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="n"></param>
        /// <returns>Only returns plausible candidates, if all are bad, the result is empty</returns>
        private List<Point> GetCandidates(int x, int y, List<PointMapping> n)
        {
            if (n.Count < 3) throw new ArgumentException("at least 3 neighbours needed");
            var targ = new Point(x, y);
            var poss = new List<Point>();
            for (int i = 0; i < n.Count - 2; i++)
            {
                for (int j = i + 1; j < n.Count; j++)
                {
                    for (int k = j + 1; k < n.Count; k++)
                    {
                        var b = new BarycentricCoordinate(targ, n[i].Image, n[j].Image, n[k].Image);
                        if (b.IsNearby)
                            poss.Add(b.GetCartesianCoordinates(n[i].Screen, n[j].Screen, n[k].Screen));
                    }
                }
            }
            return poss;
        }

        /// <summary>
        /// Picks the nearest Values from the SortedDictionary
        /// </summary>
        /// <typeparam name="T">Type of the value</typeparam>
        /// <param name="src"></param>
        /// <param name="target">target key</param>
        /// <param name="count">number of desired values</param>
        /// <returns>selected range</returns>
        private SortedDictionary<int, T> PickNearest<T>(SortedDictionary<int, T> src, int target, int count)
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
        /// <summary>
        /// Picking the n nearest points
        /// </summary>
        /// <param name="x">target x coordinate</param>
        /// <param name="y">target y coordinate</param>
        /// <param name="desired">number of neighbours</param>
        /// <returns>ordered by distance</returns>
        private List<PointMapping> FindNearest(int x, int y, int desired)
        {
            var cols = PickNearest(_calibratorData, x, desired);
            var p = new List<PointMapping>();
            foreach (var col in cols)
            {
                var tmp = PickNearest(col.Value, y, desired);
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
        /// Average position
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        private Point Average(List<Point> points)
        {
            if(points.Count == 0)
                throw new ArgumentException("List is empty");
            if (points.Count == 1)
                return points.First();
            var res = new Point(points.Sum(x => x.X), points.Sum(x => x.Y));
            res.X = (int)Math.Round(res.X * 1.0 / points.Count);
            res.Y = (int)Math.Round(res.Y * 1.0 / points.Count);
            return res;
        }

        /// <summary>
        /// Weighted Average
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        private Point Average(Dictionary<Point, double> points)
        {
            if (points.Count == 0)
                throw new ArgumentException("List is empty");
            if (points.Count == 1)
                return points.First().Key;
            var rx = points.Sum(x => x.Value * x.Key.X);
            var ry = points.Sum(x => x.Value * x.Key.Y);
            var sWeights = points.Sum(x => x.Value);
            return new Point((int)Math.Round(rx / sWeights), (int)Math.Round(ry / sWeights));
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
        /// Calculating the average of all calibrator-data
        /// </summary>
        private void SetMapData()
        {
            for (int i = 0; i < _mapData.GetLength(0); i++)
            {
                for (int j = 0; j < _mapData.GetLength(1); j++)
                {
                    if (_calibratorData.ContainsKey(i) && _calibratorData[i].ContainsKey(j) && _calibratorData[i][j].Count > 0)
                    {
                        try
                        {
                            //var k = new Point();
                            //k.X = _calibratorData[i][j].Sum(x => x.X)/_calibratorData[i][j].Count;
                            //k.Y = _calibratorData[i][j].Sum(x => x.Y)/_calibratorData[i][j].Count;
                            _mapData[i, j] = Average(_calibratorData[i][j]);
                        }
                        catch (Exception e)
                        {
                            Debug.WriteLine(e.Message + " happend during grid calculation at " + i + ", " + j);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Calculate from corners
        /// </summary>
        public void PredictFromCorners()
        {
            // TODO index out of bound exception möglich
            int xmax = ScreenSize.Width;
            int ymax = ScreenSize.Height;
            var stor = _calibratorData;
            _calibratorData = new SortedDictionary<int, SortedDictionary<int, List<Point>>>(_calibratorData);
            //copy calibrator data after backup
            for (int i = 0; i < xmax; i+=2)
            {
                //Debug.WriteLine(i);
                for (int j = 0; j < ymax; j+=2)
                {
                //Debug.WriteLineIf(i > 1278, j);
                    //find intersection
                    var y1 = (int)Math.Round(TopLeft.Y - (TopLeft.Y - TopRight.Y) * (i / (double)xmax));
                    var y2 = (int)Math.Round(BottomLeft.Y - (BottomLeft.Y - BottomRight.Y) * (i / (double)xmax));
                    var x1 = (int)Math.Round(TopLeft.X - (TopLeft.X - TopRight.X) * (i / (double)xmax));
                    var x2 = (int)Math.Round(BottomLeft.X - (BottomLeft.X - BottomRight.X) * (i / (double)xmax));
                    var y3 = (int)Math.Round(TopLeft.Y - (TopLeft.Y - BottomLeft.Y) * (j / (double)ymax));
                    var y4 = (int)Math.Round(TopRight.Y - (TopRight.Y - BottomRight.Y) * (j / (double)ymax));
                    var x3 = (int)Math.Round(TopLeft.X - (TopLeft.X - BottomLeft.X) * (j / (double)ymax));
                    var x4 = (int)Math.Round(TopRight.X - (TopRight.X - BottomRight.X) * (j / (double)ymax));
                    var a1 = y2 - y1;
                    var b1 = x1 - x2;
                    var c1 = a1*x1 + b1*y1;
                    var a2 = y4 - y3;
                    var b2 = x3 - x4;
                    var c2 = a2 * x3 + b2 * y3;
                    double det = a1*b2 - a2*b1;
                        var x = (int) Math.Round((b2*c1 - b1*c2)/det);
                        var y = (int) Math.Round((a1*c2 - a2*c1)/det);
                    try
                    {
                        AddCalibratorPoint(x, y, new Point(i,j));
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine("Tried to add " + x + ", " + y + " and " + e.Message + " happend");
                    }
                }
            }
            SetMapData();
            //restoring backup
            _calibratorData = stor;
        }

        /// <summary>
        /// Get the calculated screen coordinates
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public Point GetPosition(int x, int y)
        {
            return _mapData[x, y];
        }

        /// <summary>
        /// Get Barycentric interpolation based on the corners
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public Point PredictPosition(int x, int y)
        {
            var b1 = new BarycentricCoordinate(new Point(x, y), TopLeft, TopRight, BottomLeft);
            var p1 = b1.GetCartesianCoordinates(new Point(ScreenSize.Left,ScreenSize.Top),
                new Point(ScreenSize.Right, ScreenSize.Top), new Point(ScreenSize.Left, ScreenSize.Bottom));
            //return p1;
            //if (b1.IsInside)
            //    return p1;
            var b2 = new BarycentricCoordinate(new Point(x, y), TopLeft, BottomLeft, BottomRight);
            var p2 = b2.GetCartesianCoordinates(new Point(ScreenSize.Left,ScreenSize.Top),
                new Point(ScreenSize.Left, ScreenSize.Bottom), new Point(ScreenSize.Right, ScreenSize.Bottom));
            var b3 = new BarycentricCoordinate(new Point(x, y), TopRight, BottomLeft, BottomRight);
            var p3 = b3.GetCartesianCoordinates(new Point(ScreenSize.Right, ScreenSize.Top),
                new Point(ScreenSize.Left, ScreenSize.Bottom), new Point(ScreenSize.Right, ScreenSize.Bottom));
            var b4 = new BarycentricCoordinate(new Point(x, y), TopLeft, TopRight, BottomRight);
            var p4 = b4.GetCartesianCoordinates(new Point(ScreenSize.Left, ScreenSize.Top),
                new Point(ScreenSize.Right, ScreenSize.Top), new Point(ScreenSize.Right, ScreenSize.Bottom));
            //return p2;
            //if (b2.IsInside)
            //{
            //    return p2;
            //}
            if (!(b1.IsInside || b3.IsInside || b2.IsInside || b4.IsInside))
                return new Point(-1, -1);
            return new Point((p1.X + p2.X + p3.X + p4.X)/4, (p1.Y + p2.Y + p3.Y + p4.Y)/4);
            //if (!b1.IsInside && !b2.IsInside)
            //    return new Point();
            //return new Point((int) Math.Round((p1.X + p2.X)/2.0), (int) Math.Round((p1.Y + p2.Y)/2.0));
        }

        /// <summary>
        /// Interpolate Barycentric, based on next neighbours
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public Point InterpolatePosition(int x, int y)
        {
            return Interpolate(x, y);
        }
        /// <summary>
        /// Checks if the point is in the Grid
        /// </summary>
        /// <param name="centerOfGravity"></param>
        /// <returns></returns>
        public bool Contains(AForge.Point centerOfGravity)
        {
            return Contains(new Point((int) centerOfGravity.X, (int) centerOfGravity.Y));
        }

        /// <summary>
        /// Perdict the position based on the corners
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public Point PredictPosition(AForge.Point p)
        {
            return PredictPosition((int) p.X, (int) p.Y);
        }

        /// <summary>
        /// Interpolate based on nearest neighbours
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public Point InterpolatePosition(AForge.Point point)
        {
            return InterpolatePosition((int) Math.Round(point.X), (int) Math.Round(point.Y));
        }

        public Point GetPosition(AForge.Point point)
        {
            return GetPosition((int) point.X, (int) point.Y);
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

    /// <summary>
    /// Calculates Barycentric coordinates and remapping
    /// </summary>
    public class BarycentricCoordinate
    {
        public double Lambda1 { get; set; }

        public double Lambda2 { get; set; }

        public double Lambda3 { get; set; }

        /// <summary>
        /// Calculate from corners
        /// </summary>
        /// <param name="target">target point</param>
        /// <param name="corner1"></param>
        /// <param name="corner2"></param>
        /// <param name="corner3"></param>
        public BarycentricCoordinate(Point target, Point corner1, Point corner2, Point corner3)
        {
            var den = 1.0 / ((corner2.Y - corner3.Y) * (corner1.X - corner3.X) + (corner3.X - corner2.X) * (corner1.Y - corner3.Y));
            Lambda1 = ((corner2.Y - corner3.Y) * (target.X - corner3.X) + (corner3.X - corner2.X) * (target.Y - corner3.Y)) * den;
            Lambda2 = ((corner3.Y - corner1.Y) * (target.X - corner3.X) + (corner1.X - corner3.X) * (target.Y - corner3.Y)) * den;
            Lambda3 = 1.0 - Lambda1 - Lambda2;
            //Debug.WriteLine("L1: " + Lambda1 + " L2: " + Lambda2 + " L3: " + Lambda3);
        }

        /// <summary>
        /// True if the point is inside the triangle
        /// </summary>
        public bool IsInside
        { get { return Lambda1 >= 0 && Lambda1 <= 1 && Lambda2 >= 0 && Lambda2 <= 1 && Lambda3 >= 0 && Lambda3 <= 1; } }

        /// <summary>
        /// True if all lambdas smaller 1.5
        /// </summary>
        public bool IsNearby
        { get { return Lambda1 <= 1.5 && Lambda2 <= 1.5 && Lambda3 <= 1.5; } }

        /// <summary>
        /// Rebasing the point
        /// </summary>
        /// <param name="corner1"></param>
        /// <param name="corner2"></param>
        /// <param name="corner3"></param>
        /// <returns></returns>
        public Point GetCartesianCoordinates(Point corner1, Point corner2, Point corner3)
        {
            return new Point((int) Math.Round(corner1.X*Lambda1 + Lambda2*corner2.X + corner3.X*Lambda3),
                             (int) Math.Round(corner1.Y*Lambda1 + Lambda2*corner2.Y + corner3.Y*Lambda3));
        }
    }
}
