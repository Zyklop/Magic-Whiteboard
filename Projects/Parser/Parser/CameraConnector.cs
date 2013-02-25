using System;
using Parser.Events;
using Parser.Mock;

namespace Parser
{
    public class CameraConnector
    {
        public CameraConnector(Camera camera)
        {
        }

        protected CameraConnector()
        {
        }

        public virtual event EventHandler<NewImageEventArgs> NewImage;
    }
}
