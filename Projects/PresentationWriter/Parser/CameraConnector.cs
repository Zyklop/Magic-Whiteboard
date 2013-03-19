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

        /// <summary>
        /// Facade to get images from the camera
        /// </summary>
        /// <param name="camera"></param>
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

        /// <summary>
        /// Notified if new images avaliable
        /// </summary>
        public virtual event EventHandler<NewImageEventArgs> NewImage;
    }
}
