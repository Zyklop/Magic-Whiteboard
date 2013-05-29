using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("IntegralPointMapper:");
            IntegralPointMapperTest.Run();

            Console.WriteLine("BarycentricIntegralPointMapper:");
            BarycentricIntegralPointMapperTest.Run();

            Console.ReadLine();
        }
    }
}
