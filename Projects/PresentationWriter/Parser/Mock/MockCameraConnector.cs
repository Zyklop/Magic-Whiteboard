using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Timers;
using System.Threading;
using System.Threading.Tasks;
using HSR.PresentationWriter.Parser;
using HSR.PresentationWriter.Parser.Events;
using HSR.PresentationWriter.Parser.Images;
using System.Drawing;
using Timer = System.Timers.Timer;

namespace Parser.Mock
{

    public class MockCameraConnector:CameraConnector
    {
        private System.Timers.Timer tim = new Timer();
        private List<Bitmap> imagelist;
        private int index=0;

        public MockCameraConnector()
        {
            imagelist = new List<Bitmap>();
            for (int i = 1; i < 17; i++)
            {
                imagelist.Add(new Bitmap(@"C:\temp\inp\img" + i + ".jpg"));
            }
            Start();
        }

        private void Start()
        {
            tim.Interval = 5000;
            tim.AutoReset = true;
            tim.Elapsed += delegate
                {
                    if (NewImage != null) NewImage(this, new NewImageEventArgs {NewImage = imagelist[index]});
                    if (++index >= imagelist.Count) index = 0;
                    //Thread.Sleep(0);
                };
            tim.Start();
        }

        public override event EventHandler<NewImageEventArgs> NewImage;
    }
}
