using System;
using HSR.PresentationWriter.DataSources;
using HSR.PresentationWriter.Parser.Events;

namespace HSR.PresentationWriter.Parser
{
    public class CameraConnector
    {
        private AvicapCamera _camera;

        public CameraConnector(AvicapCamera camera)
        {
            _camera = camera;
            _camera.Start();
        }

        protected CameraConnector()
        {
            //for mocking, no other use
        }

        public virtual event EventHandler<NewImageEventArgs> NewImage;
    }
}
