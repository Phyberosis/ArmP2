using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Leap;

namespace Controllers
{
    class Test
    {
        private Leap.Controller leap;

        public static void Main()
        {
            Console.WriteLine("Controller test");
            new Test();
            Console.WriteLine("end");
            Console.ReadLine();
        }

        private Test()
        {
            leap = new Leap.Controller();
            leap.EventContext = SynchronizationContext.Current;
            leap.FrameReady += frameHandler;
        }

        private void frameHandler(object sender, FrameEventArgs args)
        {
            Frame frame = args.frame;
            string msg = frame.Id.ToString();
            msg += frame.Hands.Count;
            Console.WriteLine(msg);
        }
    }
}
