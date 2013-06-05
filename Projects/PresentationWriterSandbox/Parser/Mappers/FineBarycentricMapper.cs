using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AForge;

namespace HSR.PresWriter.PenTracking.Mappers
{
    public class FineBarycentricMapper:AbstractPointMapper
    {
        private readonly int[] _iterationNeighbours = { 10, 15, 19, 24, 30 };
        //private int[] iterationNeighbours = {4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30 };
        private const int NeighboursNeeded = 7;
#if DEBUG
        public int[] NeighbourUsedCount { get; set; }
#endif

        public FineBarycentricMapper(Grid grid) : base(grid)
        {
#if DEBUG
            using (var fs = new StreamWriter(new FileStream(@"C:\Temp\aforge\points.csv", FileMode.Create, FileAccess.Write)))
            {
                fs.WriteLine("ScreenX,ScreenY,ImageX,ImageY");
                fs.Flush();
            }
            NeighbourUsedCount = new int[_iterationNeighbours.Length];
#endif
        }

        public override Point FromPresentation(Point p)
        {
            var x = (int)Math.Round(p.X);
            var y = (int)Math.Round(p.Y);
            var poss = new List<System.Drawing.Point>();
            List<PointMapping> n;
            for (int i = 0; i < _iterationNeighbours.Length && poss.Count < NeighboursNeeded; i++)
            {
                n = Grid.FindNearest(x, y, _iterationNeighbours[i]);
                poss = GetCandidates(x, y, n);
#if DEBUG
                if (poss.Count >= NeighboursNeeded)
                {
                    NeighbourUsedCount[i]++;
                }
#endif
            }
            var a = Average(poss);
            return new Point(a.X, a.Y);
        }


        /// <summary>
        /// Average position
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        private System.Drawing.Point Average(List<System.Drawing.Point> points)
        {
            if (points.Count == 0)
                throw new ArgumentException("List is empty");
            if (points.Count == 1)
                return points.First();
            var res = new System.Drawing.Point(points.Sum(x => x.X), points.Sum(x => x.Y));
            res.X = (int)Math.Round(res.X * 1.0 / points.Count);
            res.Y = (int)Math.Round(res.Y * 1.0 / points.Count);
            return res;
        }

        /// <summary>
        /// Weighted Average
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        private System.Drawing.Point Average(Dictionary<System.Drawing.Point, double> points)
        {
            if (points.Count == 0)
                throw new ArgumentException("List is empty");
            if (points.Count == 1)
                return points.First().Key;
            var rx = points.Sum(x => x.Value * x.Key.X);
            var ry = points.Sum(x => x.Value * x.Key.Y);
            var sWeights = points.Sum(x => x.Value);
            return new System.Drawing.Point((int)Math.Round(rx / sWeights), (int)Math.Round(ry / sWeights));
        }



        /// <summary>
        /// Predict from each possible triange, returning each candidate
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="n"></param>
        /// <returns>Only returns plausible candidates, if all are bad, the result is empty</returns>
        internal List<System.Drawing.Point> GetCandidates(int x, int y, List<PointMapping> n)
        {
            if (n.Count < 3) throw new ArgumentException("at least 3 neighbours needed");
            var targ = new System.Drawing.Point(x, y);
            var poss = new List<System.Drawing.Point>();
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
    }
}
