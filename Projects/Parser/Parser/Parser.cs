using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser
{
    public class Parser
    {
        private CameraConnector _cc;

        public Parser(Camera camera)
        {
            _cc = new CameraConnector(camera);
        }


    }
}
