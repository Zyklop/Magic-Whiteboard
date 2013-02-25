using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Parser;
using Parser.Mock;

namespace MockTester
{
    class Program
    {
        static void Main(string[] args)
        {
            var parser = new DataParser(new MockCameraConnector());
            parser.Initialize();
            parser.Start();
            parser.PenPositionChanged += delegate { Console.WriteLine("new image"); };
            while(true)
                System.Threading.Thread.Sleep(5);
        }
    }
}
