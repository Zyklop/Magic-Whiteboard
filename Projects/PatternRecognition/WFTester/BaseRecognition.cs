using AForge.Imaging;
using AForge.Imaging.Filters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WFTester
{
    class BaseRecognition
    {
        public static List<Bitmap> Recognize()
        {
            DirectoryInfo source = new DirectoryInfo(@"c:\temp\recognition");
            if (Directory.Exists(source.FullName) == false)
            {
                Directory.CreateDirectory(source.FullName);
            }

            Bitmap baseBitmap = new Bitmap(Path.Combine(source.FullName, @"01_base.jpg"));
            Bitmap armBitmap = new Bitmap(Path.Combine(source.FullName, @"02_point_right_arm.jpg"));
            Bitmap fingerBitmap = new Bitmap(Path.Combine(source.FullName, @"03_point_right_finger.jpg"));

            Grayscale grayscaleFilter = new GrayscaleBT709();
            Bitmap baseBitmapGray   = grayscaleFilter.Apply(baseBitmap);
            Bitmap armBitmapGray    = grayscaleFilter.Apply(armBitmap);
            Bitmap fingerBitmapGray = grayscaleFilter.Apply(fingerBitmap);

            Difference differenceFilter = new Difference(baseBitmapGray);
            Bitmap difference1 = differenceFilter.Apply(armBitmapGray);
            Bitmap difference2 = differenceFilter.Apply(fingerBitmapGray);

            Difference differenceFilter2 = new Difference(armBitmapGray);
            Bitmap difference3 = differenceFilter2.Apply(fingerBitmapGray);

            return new List<Bitmap>() {
                baseBitmapGray, armBitmapGray, difference1, difference2, difference3
            };
        }

        public static List<Bitmap> RecognizeLines()
        {
            Bitmap b = new Bitmap(@"c:\temp\recognition\diff-arm.png");

            Grayscale grascaleFilter = new GrayscaleBT709();

            Threshold t = new Threshold(20);
            Bitmap tBitmap = t.Apply(grascaleFilter.Apply(b));

            HoughLineTransformation lineTransform = new HoughLineTransformation();
            lineTransform.ProcessImage(tBitmap);
            Bitmap houghLineImage = lineTransform.ToBitmap();
            // get lines using relative intensity
            HoughLine[] lines = lineTransform.GetLinesByRelativeIntensity(0.4);

            Bitmap linesBitmap = new Bitmap(b);
            foreach (HoughLine line in lines)
            {
                DrawHoughLine(line, linesBitmap);
            }

            return new List<Bitmap>() {
                b, tBitmap, houghLineImage, linesBitmap
            };
        }

        public static void DrawHoughLine(HoughLine line, Bitmap toImage)
        {
            // get line's radius and theta values
            int r = line.Radius;
            double t = line.Theta;

            // check if line is in lower part of the image
            if (r < 0)
            {
                t += 180;
                r = -r;
            }

            // convert degrees to radians
            t = (t / 180) * Math.PI;

            // get image centers (all coordinate are measured relative
            // to center)
            int w2 = toImage.Width / 2;
            int h2 = toImage.Height / 2;

            double x0 = 0, x1 = 0, y0 = 0, y1 = 0;

            if (line.Theta != 0)
            {
                // none-vertical line
                x0 = -w2; // most left point
                x1 = w2;  // most right point

                // calculate corresponding y values
                y0 = (-Math.Cos(t) * x0 + r) / Math.Sin(t);
                y1 = (-Math.Cos(t) * x1 + r) / Math.Sin(t);
            }
            else
            {
                // vertical line
                x0 = line.Radius;
                x1 = line.Radius;

                y0 = h2;
                y1 = -h2;
            }

            // draw line on the image
            using (Graphics g = Graphics.FromImage(toImage))
            {
                g.DrawLine(Pens.Red, (int)x0 + w2, h2 - (int)y0, (int)x1 + w2, h2 - (int)y1);
                g.Save();
            }
        }
    }
}
