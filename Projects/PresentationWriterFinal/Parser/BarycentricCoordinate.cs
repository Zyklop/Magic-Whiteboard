using System;
using System.Drawing;

namespace HSR.PresWriter.PenTracking
{
    /// <summary>
    /// Calculates Barycentric coordinates and remapping
    /// </summary>
    internal class BarycentricCoordinate
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
            return new Point((int)Math.Round(corner1.X * Lambda1 + Lambda2 * corner2.X + corner3.X * Lambda3),
                             (int)Math.Round(corner1.Y * Lambda1 + Lambda2 * corner2.Y + corner3.Y * Lambda3));
        }
    }
}
