using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSR.PresentationWriter.Parser
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
        private SortedDictionary<int, SortedSet<int>> _refPoints; 

        public Grid(int width, int height)
        {
            _mapData = new Point[width,height];
            _calibratorData = new List<Point>[width,height];
            _refPoints = new SortedDictionary<int, SortedSet<int>>();
        }

        public int Width { get
        {
            return (int) (TopRight.X - TopLeft.X > BottomRight.X - BottomLeft.X
                              ? TopRight.X - TopLeft.X
                              : BottomRight.X - BottomLeft.X);
        } }

        public int Height
        {
            get
            {
                return (int) (BottomLeft.Y - TopLeft.Y > BottomRight.Y - TopRight.Y
                                  ? BottomLeft.Y - TopLeft.Y
                                  : BottomRight.Y - TopRight.Y);
            }
        }

        public bool DataNeeded { get { return _needed; } }

        public Rect Needed()
        {
            return new Rect();
        }

        public Point TopLeft
        {
            get { return _topLeft; }
            set { 
                _topLeft = value;
                _calibratorData[(int) value.X,(int) value.Y].Add(new Point(0,0));
                AddRefPoints(0,0);
            }
        }

        public Point TopRight
        {
            get { return _topRight; }
            set
            {
                _topRight = value;
            }
        }

        public Point BottomLeft
        {
            get { return _bottomLeft; }
            set
            {
                _bottomLeft = value;
            }
        }

        public Point BottomRight
        {
            get { return _bottomRight; }
            set
            {
                _bottomRight = value;
            }
        }

        public void Reset()
        {
            foreach (List<Point> l in _calibratorData)
            {
                l.Clear();
            }
        }

        public void AddPoint(int screenX, int screenY, int imgX, int imgY)
        {
            _calibratorData[imgX,imgY].Add(new Point{X = screenX, Y = screenY});
            AddRefPoints(screenX, screenY);
        }

        private void AddRefPoints(int screenX, int screenY)
        {
            if (_refPoints.ContainsKey(screenX))
            {
                _refPoints[screenX].Add(screenY);
            }
            else
            {
                _refPoints.Add(screenX, new SortedSet<int>());
                _refPoints[screenX].Add(screenY);
            }
        }

        public void Calculate()
        {
            for (int i = 0; i < _mapData.GetLength(0); i++)
            {
                for (int j = 0; j < _mapData.GetLength(1); j++)
                {
                    var k = new Point();
                    k.X = _calibratorData[i, j].Sum(x => x.X)/_calibratorData[i, j].Count;
                    k.Y = _calibratorData[i, j].Sum(x => x.Y)/_calibratorData[i, j].Count;
                    _mapData[i, j] = k;
                }
            }
            _refPoints.Clear();
        }

        public Point GetPosition(int x, int y)
        {
            return _mapData[x, y];
        }
    }
}
