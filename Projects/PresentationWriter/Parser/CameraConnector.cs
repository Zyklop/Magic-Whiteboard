using System;
using System.Drawing;
using HSR.PresentationWriter.DataSources;
using HSR.PresentationWriter.Parser.Events;
using HSR.PresentationWriter.DataSources;
using HSR.PresentationWriter.Parser.Images;

namespace HSR.PresentationWriter.Parser
{
    public class CameraConnector
    {
        private ICamera _camera;

        public CameraConnector(ICamera camera)
        {
            _camera = camera;
            _camera.FrameReady += NewFrame;
            _camera.Start();
        }

        private void NewFrame(object sender, DataSources.FrameReadyEventArgs e)
        {
            NewImage(this, new NewImageEventArgs{NewImage = new ThreeChannelBitmap(e.Frame.Image)});
        }

        protected CameraConnector()
        {
            //for mocking, no other use
        }

        public virtual event EventHandler<NewImageEventArgs> NewImage;
    }
}
