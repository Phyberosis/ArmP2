using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Data;
using Leap;

namespace Input
{
    internal class Test
    {

        private Leap.Controller leap;

        public static void Main()
        {
            Console.WriteLine("Controller test");
            Console.WriteLine(Directory.GetCurrentDirectory());
            Test t = new Test();
            Console.WriteLine("end");
            Console.ReadLine();
            t.leap.Dispose();
        }

        private Test()
        {
            leap = new Leap.Controller();
            leap.EventContext = SynchronizationContext.Current;
            leap.FrameReady += frameHandler;
            leap.Connect += con;
        }

        private void con(object sender, ConnectionEventArgs e)
        {
            Console.WriteLine("connected");
        }

        float last = Time.Now();
        private void frameHandler(object sender, FrameEventArgs args)
        {
            Frame frame = args.frame;
            if (frame.Hands.Count == 0) return;

            Hand h = frame.Hands.First();
            Vector lPos = h.PalmPosition;
            Vector3 pos = new Vector3(lPos.x, -lPos.z, lPos.y);
            string msg = pos.ToString();
            LeapQuaternion lq = h.Rotation;
            Quaternion q = new Quaternion(lq.x, -lq.z, lq.y, lq.w);
            msg += "\n" + Vector3.Transform(Vector3.UnitY, q) + "\n" + Vector3.Transform(Vector3.UnitZ, q) + "\n";
            if (Time.Now() - last < 1f) return;
            Console.WriteLine(msg);
            last = Time.Now();

            leap.RequestImages(frame.Id, Image.ImageType.DEFAULT);
        }
    }
}