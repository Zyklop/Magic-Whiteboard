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

        private async void NewFrame(object sender, FrameReadyEventArgs e)
        {
            NewImage(this, new NewImageEventArgs{NewImage = e.Frame.Bitmap});
        }

        protected CameraConnector()
        {
            //for mocking, no other use
        }

        public virtual event EventHandler<NewImageEventArgs> NewImage;
    }
}
