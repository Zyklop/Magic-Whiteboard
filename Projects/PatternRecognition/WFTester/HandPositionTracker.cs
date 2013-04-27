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
    class HandPositionTracker
    {
        private UnmanagedImage _baseImage;
        private UnmanagedImage _bottomBaseImage;
        private Rectangle BOTTOM_BASE_STRIPE = new Rectangle(0, 480 - 50, 640, 50);
        private List<UnmanagedImage> _images;

        public HandPositionTracker(String baseImageFile, String frameImagePath)
        {
            DirectoryInfo source = new DirectoryInfo(frameImagePath);
            if (Directory.Exists(source.FullName) == false)
            {
                Directory.CreateDirectory(source.FullName);
            }

            Bitmap baseBitmap = new Bitmap(baseImageFile);
            _baseImage = UnmanagedImage.FromManagedImage(baseBitmap);

            // Cut Bottom for later use
            Crop cropFilter = new Crop(BOTTOM_BASE_STRIPE);
            _bottomBaseImage = cropFilter.Apply(_baseImage);
            _bottomBaseImage.ToManagedImage().Save(@"c:\temp\recognition\bottombase.png");

            // Load Images
            _images = new List<UnmanagedImage>();
            foreach (FileInfo i in source.GetFiles("*.png"))
            {
                Bitmap b = new Bitmap(i.FullName);
                _images.Add(UnmanagedImage.FromManagedImage(b));
            }
        }

        public void SetBaseImage(UnmanagedImage img)
        {
            _baseImage = img;
        }

        // Slow version (eventually pictures could be resized for more performance)
        private void FindPersonPosition(UnmanagedImage frameImage)
        {
            var stopwatch = Stopwatch.StartNew();

            Difference diffFilter = new Difference(_baseImage);
            UnmanagedImage diffImg = diffFilter.Apply(frameImage);
            //diffImg.ToManagedImage().Save(@"c:\temp\recognition\diff.png");

            // TODO ev. ColorFilter
            Grayscale grayFilter = new GrayscaleBT709();
            diffImg = grayFilter.Apply(diffImg);
            //diffImg.ToManagedImage().Save(@"c:\temp\recognition\diff-gray.png");

            Threshold thresholdFilter = new Threshold(30);
            thresholdFilter.ApplyInPlace(diffImg);
            //diffImg.ToManagedImage().Save(@"c:\temp\recognition\diff-gray-thres.png");

            Median medianFilter = new Median(3);
            medianFilter.ApplyInPlace(diffImg);
            //diffImg.ToManagedImage().Save(@"c:\temp\recognition\diff-gray-thres-med.png");

            // Find Person Borders (TODO ev. Performance Boost, wenn CollectActivePixels vermieden wird)
            // per Iteration durch Pixeldaten koennte ev. die groesste und die kleinste x-Koordinate schneller
            // gefunden werden..
            List<AForge.IntPoint> whites = diffImg.CollectActivePixels(BOTTOM_BASE_STRIPE);
            int minX = whites.Min(obj => obj.X);
            int maxX = whites.Max(obj => obj.X);

            // Schneller als MS Algorithmus fuer Min und Max
            // (auch wenn es assymptotisch gleich schnell sein muesste)
            //int minX = int.MaxValue;
            //int maxX = int.MinValue;
            //foreach(AForge.IntPoint whitePixel in whites)
            //{
            //    //if (whitePixel.X < minX)
            //    //{
            //    //    minX = whitePixel.X;
            //    //}
            //    if (whitePixel.X > maxX)
            //    {
            //        maxX = whitePixel.X;
            //    }
            //}

            var elapsedStage1 = stopwatch.ElapsedMilliseconds;

            Debug.WriteLine("Person is this fat: {0}, {1}", minX, maxX);

            // Find Arm
            whites = diffImg.CollectActivePixels(new Rectangle(maxX, 0, 640-maxX, 480-BOTTOM_BASE_STRIPE.Height));
            maxX = whites.Max(obj => obj.X);
            var armPoint = whites.First(obj => obj.X == maxX);
            Debug.WriteLine("Persons arm is here: {0}, {1}", armPoint.X, armPoint.Y);

            Bitmap b = frameImage.ToManagedImage();
            using (Graphics g = Graphics.FromImage(b))
            {
                g.DrawEllipse(Pens.Red, armPoint.X, armPoint.Y, 5, 5);
                b.Save(@"c:\temp\recognition\gach.png");
            }

            Debug.WriteLine("Time: {0}", elapsedStage1);
            stopwatch.Stop();
        }

        //// Fast version
        //private void FindPersonPosition(UnmanagedImage frameImage)
        //{
        //    var stopwatch = Stopwatch.StartNew();

        //    Crop cropFilter = new Crop(BOTTOM_BASE_STRIPE);
        //    UnmanagedImage diffImg = cropFilter.Apply(frameImage);
        //    //diffImg.ToManagedImage().Save(@"c:\temp\recognition\crop.png");

        //    Difference diffFilter = new Difference(_bottomBaseImage);
        //    diffFilter.ApplyInPlace(diffImg);
        //    //diffImg.ToManagedImage().Save(@"c:\temp\recognition\diff.png");

        //    // TODO ev. ColorFilter
        //    Grayscale grayFilter = new GrayscaleBT709();
        //    diffImg = grayFilter.Apply(diffImg);
        //    //diffImg.ToManagedImage().Save(@"c:\temp\recognition\diff-gray.png");

        //    Threshold thresholdFilter = new Threshold(30);
        //    thresholdFilter.ApplyInPlace(diffImg);
        //    //diffImg.ToManagedImage().Save(@"c:\temp\recognition\diff-gray-thres.png");

        //    Median medianFilter = new Median(3);
        //    medianFilter.ApplyInPlace(diffImg);
        //    //diffImg.ToManagedImage().Save(@"c:\temp\recognition\diff-gray-thres-med.png");

        //    var elapsedStage1 = stopwatch.ElapsedMilliseconds;

        //    // Find Person Borders (TODO ev. Performance Boost, wenn CollectActivePixels vermieden wird)
        //    // per Iteration durch Pixeldaten koennte ev. die groesste und die kleinste x-Koordinate schneller
        //    // gefunden werden..
        //    List<AForge.IntPoint> whites = diffImg.CollectActivePixels();
        //    int minX = whites.Min(obj => obj.X);
        //    int maxX = whites.Max(obj => obj.X);

        //    // Schneller als MS Algorithmus fuer Min und Max
        //    // (auch wenn es assymptotisch gleich schnell sein muesste)
        //    //int minX = int.MaxValue;
        //    //int maxX = int.MinValue;
        //    //foreach(AForge.IntPoint whitePixel in whites)
        //    //{
        //    //    //if (whitePixel.X < minX)
        //    //    //{
        //    //    //    minX = whitePixel.X;
        //    //    //}
        //    //    if (whitePixel.X > maxX)
        //    //    {
        //    //        maxX = whitePixel.X;
        //    //    }
        //    //}
        //    var elapsedStage1 = stopwatch.ElapsedMilliseconds;
        //
        //    Debug.WriteLine("Person is this fat: {0}, {1}", minX, maxX);
        //    Debug.WriteLine("Time: {0}", elapsedStage1);
        //    stopwatch.Stop();
        //}

        public void RecognizeOne(int i)
        {
            UnmanagedImage img = _images[i % _images.Count];
            this.FindPersonPosition(img);
        }

        public void RecognizeAll()
        {
            foreach (UnmanagedImage img in _images)
            {
                this.FindPersonPosition(img);
            }
        }
    }
}
