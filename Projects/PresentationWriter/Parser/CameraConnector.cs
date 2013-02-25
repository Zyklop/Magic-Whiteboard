using System;
using HSR.PresentationWriter.DataSources;
using HSR.PresentationWriter.Parser.Events;

namespace HSR.PresentationWriter.Parser
{
    public class CameraConnector
    {
        private Camera _camera;

        public CameraConnector(Camera camera)
        {
            _camera = camera;
            _camera.Start();
        }

        protected CameraConnector()
        {
        }

        public virtual event EventHandler<NewImageEventArgs> NewImage;
    }
}
