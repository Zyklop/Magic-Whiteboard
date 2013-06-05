using AForge;
using HSR.PresWriter.PenTracking;
using HSR.PresWriter.PenTracking.Mappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    public class IntegralPointMapperTest
    {
        public class MockGrid : Grid
        {
            public MockGrid()
                : base(640, 480)
            {
                ScreenSize  = new System.Drawing.Rectangle(0, 0, 1024, 768);
                TopLeft     = new System.Drawing.Point(265, 070);
                BottomLeft  = new System.Drawing.Point(265, 313);
                TopRight    = new System.Drawing.Point(522, 126);
                BottomRight = new System.Drawing.Point(533, 333);
            }
        }

        public static void Run()
        {
            MockGrid g = new MockGrid();
            IntegralPointMapper m = new IntegralPointMapper(g);

            Point camera, beamer;

            // Top Left
            camera = new Point(265, 70);
            beamer = m.FromPresentation(camera);
            Console.WriteLine("{0} -> {1}", camera, beamer);

            // Bottom Left
            camera = new Point(265, 313);
            beamer = m.FromPresentation(camera);
            Console.WriteLine("{0} -> {1}", camera, beamer);

            // Top Right
            camera = new Point(522, 126);
            beamer = m.FromPresentation(camera);
            Console.WriteLine("{0} -> {1}", camera, beamer);

            // Bottom Right
            camera = new Point(533, 333);
            beamer = m.FromPresentation(camera);
            Console.WriteLine("{0} -> {1}", camera, beamer);

            // Center
            camera = new Point(409, 480-270);
            beamer = m.FromPresentation(camera);
            Console.WriteLine("{0} -> {1}", camera, beamer);
        }
    }
}
