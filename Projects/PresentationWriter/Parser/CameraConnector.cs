using System;
using Parser.Events;
using Parser.Mock;
using HSR.PresentationWriter.DataSources;
using HSR.PresentationWriter.Parser.Events;

namespace HSR.PresentationWriter.Parser
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
