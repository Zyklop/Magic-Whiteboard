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
        private List<ThreeChannelBitmap> imagelist;
        private int index=0;

        public MockCameraConnector()
        {
            imagelist = new List<ThreeChannelBitmap>
                {
                    //new ThreeChannelBitmap(new Bitmap(@"Images/Raumsetup.jpg")),
                    //new ThreeChannelBitmap(new Bitmap(@"Images/Raumsetup-ohne-Licht.jpg")),
                    //new ThreeChannelBitmap(new Bitmap(@"Images/Raumsetup-ohne-Licht.jpg"))
                };
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

        public override event EventHandler<NewImageEventArgs> NewImage;
    }
}
