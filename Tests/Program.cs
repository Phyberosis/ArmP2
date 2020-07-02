using Communications;
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

            Console.WriteLine("\n\n\nComs==================\n");
            Communications.Test t1 = new Test();
            t1.run();

            Console.WriteLine("\n\n\nData==================\n");
            Data.Test t2 = new Data.Test();
            t2.run();

            Console.ReadKey();
        }
    }
}
