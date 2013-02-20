using System;

namespace Parser
{
    class CameraConnector
    {
        public CameraConnector(Camera camera)
        {
        }

        public event EventHandler<NewImageEventArgs> NewImage;
    }
}
