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
        private Point[,] _mapData;
        private bool _needed = true;
        private Point _topLeft;
        private Point _topRight;
        private Point _bottomLeft;
        private Point _bottomRight;
        private SortedDictionary<int, SortedDictionary<int, Point>> _refPoints; 

        /// <summary>
        /// Creating a mapping grid
        /// </summary>
        /// <param name="width">width of the source image</param>
        /// <param name="height">height of the source image</param>
        public Grid(int width, int height)
        {
            _mapData = new Point[width,height];
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
                    BottomRight.Y - TopLeft.Y, 
                    BottomLeft.Y - TopRight.Y, 
                    BottomLeft.Y - TopLeft.Y
                }).Max();
            }
        }

        public System.Drawing.Rectangle ScreenSize { get; set; }

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
            //foreach (var l in _calibratorData)
            //{
            //    foreach (var pl in l.Value)
            //    {
            //        pl.Value.Clear();
            //    }
            //}
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
        /// Calibrate from collected data
        /// </summary>
        public void Calculate()
        {
            int xmax = (int) ScreenSize.Width;
            int ymax = (int) ScreenSize.Height;
            for (int i = 0; i < xmax; i++)
            {
                for (int j = 0; j < ymax; j++)
                {
                    var n = FindNearest(i, j, 3);
                    var p = Interpolate(i, j, n);
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

        private Point Interpolate(int x, int y, List<PointMapping> n)
        {
            if(n.Count < 3) throw new ArgumentException("at least 3 neighbours needed");
            var targ = new Point(x, y);
            var poss = new List<Point>();
            for (int i = 0; i < n.Count - 2; i++ )
            {
                for (int j = i + 1; j < n.Count; j++ )
                {
                    for (int k = j + 1; k < n.Count; k++)
                    {
                        var b = new BarycentricCoordinate(targ, n[i].Screen, n[j].Screen, n[k].Screen);
                        poss.Add(b.GetCartesianCoordinates(n[i].Image, n[j].Image, n[k].Image));
                    }
                }
            }
            //TODO weight points
            int xSum = 0;
            int ySum = 0;
            foreach (var p in poss)
            {
                xSum += p.X;
                ySum += p.Y;
            }
            return new Point(xSum / poss.Count, ySum / poss.Count);
        }

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

        private Point Average(Dictionary<double, Point> points)
        {
            if (points.Count == 0)
                throw new ArgumentException("List is empty");
            if (points.Count == 1)
                return points.First().Value;
            var rx = points.Sum(x => x.Value.X*x.Key);
            var ry = points.Sum(x => x.Value.X * x.Key);
            var sWeights = points.Sum(x => x.Key);
            return new Point((int)Math.Round(rx / sWeights), (int)Math.Round(ry / sWeights));
        }

        private double DistanceTo(Point p1, Point p2)
        {
            var diff = new Point(p1.X - p2.X, p1.Y - p2.Y);
            return Math.Sqrt(diff.X * diff.X + diff.Y * diff.Y);
        }

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
                            var k = new Point();
                            k.X = _calibratorData[i][j].Sum(x => x.X)/_calibratorData[i][j].Count;
                            k.Y = _calibratorData[i][j].Sum(x => x.Y)/_calibratorData[i][j].Count;
                            _mapData[i, j] = k;
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
            //foreach(var i in _calibratorData)
            //{
            //    foreach (var j in i.Value)
            //    {
                    
            //    }
            //}
            for (int i = 0; i < xmax; i++)
            {
                //Debug.WriteLine(i);
                for (int j = 0; j < ymax; j++)
                {
                //Debug.WriteLineIf(i > 1278, j);
                    //find intersection
                    int y1 = (int)Math.Round(TopLeft.Y - (double)(TopLeft.Y - TopRight.Y) * ((double)i / (double)xmax));
                    int y2 = (int)Math.Round(BottomLeft.Y - (double)(BottomLeft.Y - BottomRight.Y) * ((double)i / (double)xmax));
                    int x1 = (int)Math.Round(TopLeft.X - (double)(TopLeft.X - TopRight.X) * ((double)i / (double)xmax));
                    int x2 = (int)Math.Round(BottomLeft.X - (double)(BottomLeft.X - BottomRight.X) * ((double)i / (double)xmax));
                    int y3 = (int)Math.Round(TopLeft.Y - (double)(TopLeft.Y - BottomLeft.Y) * ((double)j / (double)ymax));
                    int y4 = (int)Math.Round(TopRight.Y - (double)(TopRight.Y - BottomRight.Y) * ((double)j / (double)ymax));
                    int x3 = (int)Math.Round(TopLeft.X - (double)(TopLeft.X - BottomLeft.X) * ((double)j / (double)ymax));
                    int x4 = (int)Math.Round(TopRight.X - (double)(TopRight.X - BottomRight.X) * ((double)j / (double)ymax));
                    int A1 = y2 - y1;
                    int B1 = x1 - x2;
                    int C1 = A1*x1 + B1*y1;
                    int A2 = y4 - y3;
                    int B2 = x3 - x4;
                    int C2 = A2 * x3 + B2 * y3;
                    double det = A1*B2 - A2*B1;
                        int x = (int) Math.Round((B2*C1 - B1*C2)/det);
                        int y = (int) Math.Round((A1*C2 - A2*C1)/det);
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

        public bool Contains(AForge.Point centerOfGravity)
        {
            return Contains(new Point((int) centerOfGravity.X, (int) centerOfGravity.Y));
        }

        public Point PredictPosition(AForge.Point p)
        {
            return PredictPosition((int) p.X, (int) p.Y);
        }
    }

    public class BarycentricCoordinate
    {
        public double Lambda1 { get; set; }

        public double Lambda2 { get; set; }

        public double Lambda3 { get; set; }

        public BarycentricCoordinate(Point target, Point corner1, Point corner2, Point corner3) //:
            //this(new Point(target.X - corner1.X, target.Y - corner1.Y), 
            //new Vector(corner1.X - corner2.X, corner1.Y - corner2.Y),
            //     new Vector(corner1.X - corner3.X, corner1.Y - corner3.Y),
            //     new Vector(corner2.X - corner3.X, corner2.Y - corner3.Y))
        {
            //var v0 = new Vector(corner2.X - corner1.X, corner2.Y - corner1.Y);
            //var v1 = new Vector(corner3.X - corner1.X, corner3.Y - corner1.Y);
            //var v2 = new Vector(target.X - corner1.X, target.Y - corner1.Y);
            //var d00 = Dot(v0, v0);
            //var d01 = Dot(v0, v1);
            //var d11 = Dot(v1, v1);
            //var d20 = Dot(v2, v0);
            //var d21 = Dot(v2, v1);
            //var denom = d00 * d11 - d01 * d01;
            //Lambda1 = (d11 * d20 - d01 * d21) / denom;
            //Lambda2 = (d00 * d21 - d01 * d20) / denom;
            var den = 1.0 / ((corner2.Y - corner3.Y) * (corner1.X - corner3.X) + (corner3.X - corner2.X) * (corner1.Y - corner3.Y));
            Lambda1 = ((corner2.Y - corner3.Y) * (target.X - corner3.X) + (corner3.X - corner2.X) * (target.Y - corner3.Y)) * den;
            Lambda2 = ((corner3.Y - corner1.Y) * (target.X - corner3.X) + (corner1.X - corner3.X) * (target.Y - corner3.Y)) * den;
            Lambda3 = 1.0 - Lambda1 - Lambda2;
            //Debug.WriteLine("L1: " + Lambda1 + " L2: " + Lambda2 + " L3: " + Lambda3);
        }

        private double Dot(Vector v1, Vector v2)
        {
            return v1.X*v2.X + v1.Y + v2.Y;
        }

        //public BarycentricCoordinate(Point target, Vector v1, Vector v2, Vector v3)
        //{
        //    Lambda1 = (v2.Y - v3.Y) * (target.X - v3.X) + (v3.X - v2.X) * (target.Y - v3.Y) /
        //              ((v2.Y - v3.Y) * (v1.X - v3.X) + (v3.X - v2.X) * (v1.Y - v3.Y));
        //    Lambda2 = (v3.Y - v1.Y) * (target.X - v3.X) + (v1.X - v3.X) * (target.Y - v3.Y) /
        //        ((v2.Y - v3.Y) * (v1.X - v3.X) + (v3.X - v2.X) * (v1.Y - v3.Y));
        //    Lambda3 = 1 - Lambda1 - Lambda2;
        //}

        public bool IsInside
        { get { return Lambda1 >= 0 && Lambda1 <= 1 && Lambda2 >= 0 && Lambda2 <= 1 && Lambda3 >= 0 && Lambda3 <= 1; }}

        public Point GetCartesianCoordinates(Point corner1, Point corner2, Point corner3)
        {
            return new Point((int) Math.Round(corner1.X*Lambda1 + Lambda2*corner2.X + corner3.X*Lambda3),
                             (int) Math.Round(corner1.Y*Lambda1 + Lambda2*corner2.Y + corner3.Y*Lambda3));
        }
    }

    public struct PointMapping
    {
        public Point Screen { get; set; }

        public Point Image { get; set; }
    }
}
