using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Timers;
using System.Threading;
using System.Threading.Tasks;
using Parser;
using Parser.Events;
using Parser.Images;
using System.Drawing;
using Timer = System.Timers.Timer;

namespace Parser.Mock
{

    public class MockCameraConnector:CameraConnector
    {
        private System.Timers.Timer tim = new Timer();
        private List<ThreeChannelBitmap> imagelist;
        private int index=0;

        public MockCameraConnector() : base()
        {
            imagelist = new List<ThreeChannelBitmap>();
            imagelist.Add(new ThreeChannelBitmap(new Bitmap(@"Images/Raumsetup.jpg")));
            imagelist.Add(new ThreeChannelBitmap(new Bitmap(@"Images/Raumsetup-ohne-Licht.jpg")));
            Start();
        }

        private void Start()
        {
            tim.Interval = 33;
            tim.AutoReset = true;
            tim.Elapsed += delegate
                {
                    if (NewImage != null) NewImage(this, new NewImageEventArgs {NewImage = imagelist[index]});
                    if (++index >= imagelist.Count) index = 0;
                    Thread.Sleep(33);
                };
            tim.Start();
        }

        public override event EventHandler<Events.NewImageEventArgs> NewImage;
    }
}
