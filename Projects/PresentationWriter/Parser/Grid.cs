using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using Point = System.Drawing.Point;

namespace HSR.PresWriter.PenTracking
{
    public class Grid
    {
        private List<Point>[,] _calibratorData;
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
            _calibratorData = new List<Point>[width,height];
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

        /// <summary>
        /// More data needed for Calibration
        /// </summary>
        public bool DataNeeded { get { return _needed; } }

        /// <summary>
        /// A good area for calibration
        /// </summary>
        /// <returns></returns>
        public Rect Needed()
        {
            return new Rect();
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
            //var at = Angle(TopLeft, p, TopRight);
            //var al = Angle(TopLeft, p, BottomLeft);
            //var ar = Angle(TopRight, p, BottomRight);
            //var ab = Angle(BottomLeft, p, BottomRight);
            //return at > 180 && al < 180 && ar < 180 && ab < 180;
            // internal method
            var gp = new GraphicsPath();
            gp.AddLine((int) TopLeft.X, (int) TopLeft.Y, (int)TopRight.X, (int)TopRight.Y);
            gp.AddLine((int)TopRight.X, (int)TopRight.Y, (int)BottomRight.X, (int)BottomRight.Y);
            gp.AddLine((int)BottomRight.X, (int)BottomRight.Y, (int)BottomLeft.X, (int)BottomLeft.Y);
            gp.AddLine((int)BottomLeft.X, (int)BottomLeft.Y, (int)TopLeft.X, (int)TopLeft.Y);
            return gp.IsVisible((int) p.X,(int) p.Y);
        }

        //private double Azimuth(Point start, Point finish)
        //{
        //    var a = new Vector(p.X - left.X, p.Y - left.Y);
        //    var b = new Vector(p.X - right.X, p.Y - right.Y);
        //    return Math.Acos((a.X * b.X + a.Y * b.Y) / Math.Sqrt(a.X*a.X+a.Y*a.Y)*Math.Sqrt(b.X*b.X+b.Y*b.Y));
        //}

        /// <summary>
        /// Reset temporary calibration data
        /// </summary>
        public void Reset()
        {
            foreach (List<Point> l in _calibratorData)
            {
                l.Clear();
            }
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
            if (_calibratorData[imgX,imgY] == null)
                _calibratorData[imgX, imgY] = new List<Point>();
            _calibratorData[imgX,imgY].Add(new Point{X = screenX, Y = screenY});
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
            if (_calibratorData[img.X, img.Y] == null)
            {
                _calibratorData[img.X, img.Y] = new List<Point>();
            }
            _calibratorData[img.X, img.Y].Add(new Point { X = screen.X, Y = screen.Y });
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
            int xmax = _calibratorData[BottomRight.X, BottomRight.Y].First().X;
            int ymax = _calibratorData[BottomRight.X, BottomRight.Y].First().Y;
            for (int i = 0; i < xmax; i++)
            {
                for (int j = 0; j < ymax; j++)
                {
                    var n = FindNearest(i, j, 3);
                    var p = Interpolate(i, j, n);
                    if (_calibratorData[p.X, p.Y] != null)
                        _calibratorData[p.X,p.Y].Add(new Point(i,j));
                    else 
                        _calibratorData[p.X,p.Y] = new List<Point>{new Point(i,j)};
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

        private List<PointMapping> FindNearest(int x, int y, int desired)
        {
            //TODO optimize
            var res = new List<PointMapping>();
            int dist = 0;
               var cols = new SortedDictionary<int, SortedDictionary<int,Point>>();
               while (cols.Count < desired)
               {
                   if (_refPoints.ContainsKey(x - dist))
                       cols.Add(x - dist, _refPoints[x - dist]);
                   if (_refPoints.ContainsKey(x + dist) && dist != 0)
                       cols.Add(x + dist, _refPoints[x + dist]);
                   dist++;
               }
            dist = 0;
            var p = new List<PointMapping>();
            foreach (var col in cols)
            {
                if (col.Value.Count < desired)
                    foreach (var pair in col.Value)
                    {
                        p.Add(new PointMapping{Screen = new Point(col.Key, pair.Key), Image = pair.Value} );
                    }
                else
                {
                    int count = 0;
                    while (count < desired)
                    {
                        if (col.Value.ContainsKey(y - dist))
                        {
                            p.Add(new PointMapping{Screen = new Point(col.Key, y - dist), Image = col.Value[y - dist]});
                            count++;
                        }
                        if (col.Value.ContainsKey(y + dist) && dist != 0)
                        {
                            p.Add(new PointMapping { Screen = new Point(col.Key, y + dist), Image = col.Value[y + dist] });
                            count++;
                        }
                        dist++;
                    }
                }
            }
            p = p.OrderBy(p1 => DistanceTo(p1.Screen,new Point(x,y))).ToList();
            res.AddRange(p.GetRange(0,desired));
            return res;
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
                    if (_calibratorData[i, j] != null && _calibratorData[i, j].Count > 0)
                    {
                        try
                        {
                            var k = new Point();
                            k.X = _calibratorData[i, j].Sum(x => x.X)/_calibratorData[i, j].Count;
                            k.Y = _calibratorData[i, j].Sum(x => x.Y)/_calibratorData[i, j].Count;
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
            var stor = _calibratorData.Clone();
            int xmax = _calibratorData[BottomRight.X, BottomRight.Y].First().X;
            int ymax = _calibratorData[BottomRight.X, BottomRight.Y].First().Y;
            for (int i = 0; i < _calibratorData.GetLength(0); i++)
            {
                for (int j = 0; j < _calibratorData.GetLength(1); j++)
                {
                    if (_calibratorData[i, j]!=null)
                    {
                        _calibratorData[i, j].Clear();
                    }
                    else
                    {
                        _calibratorData[i, j] = new List<Point>();
                    }
                }
            }
            for (int i = 0; i < xmax; i++)
            {
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
                        _calibratorData[x, y].Add(new Point(i,j));
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine("Tried to add " + x + ", " + y + " and " + e.Message + " happend");
                    }
                }
            }
            SetMapData();
            _calibratorData = (List<Point>[,]) stor;
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

        public bool Contains(AForge.Point centerOfGravity)
        {
            return Contains(new Point((int) centerOfGravity.X, (int) centerOfGravity.Y));
        }
    }

    public class BarycentricCoordinate
    {
        public double Lambda1 { get; set; }

        public double Lambda2 { get; set; }

        public double Lambda3 { get; set; }

        public BarycentricCoordinate(Point target, Point corner1, Point corner2, Point corner3)
        {
            Lambda1 = (corner2.Y - corner3.Y)*(target.X - corner3.X) + (corner3.X - corner2.X)*(target.Y - corner3.Y)/
                      ((corner2.Y - corner3.Y)*(corner1.X - corner3.X) + (corner3.X - corner2.X)*(corner1.Y - corner3.Y));
            Lambda1 = (corner3.Y - corner1.Y)*(target.X - corner3.X) + (corner1.X - corner3.X)*(target.Y - corner3.Y)/
                ((corner2.Y - corner3.Y)*(corner1.X - corner3.X) + (corner3.X - corner2.X)*(corner1.Y - corner3.Y));
            Lambda3 = 1 - Lambda1 - Lambda2;
        }

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
