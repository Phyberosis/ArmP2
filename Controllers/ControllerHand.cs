using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Communications;
using Data;
using Data.Arm;

namespace Input
{
    internal class ControllerHand : LoopController
    {
        private Leap.Controller leap;
        private bool valid;
        private bool pinch;
        Vector3 startPos, refPos;
        Quaternion startQInv, refQ;
        float last = Time.Now();

        public ControllerHand(Com com) : base(com)
        {
            leap = new Leap.Controller();
            leap.FrameReady += onFrameRecieved;
            valid = false;
            pinch = false;
        }

        private void onFrameRecieved(object sender, Leap.FrameEventArgs args)
        {
            //if (Time.Now() - last < 0.05) return;
            //last = Time.Now();

            lock(this)
            {
                Leap.Frame frame = args.frame;
                if (frame.Hands.Count == 0) return;

                Leap.Hand h = frame.Hands.First();
                float p = h.PinchStrength;
                bool isFirst = false;

                if (pinch)
                {
                    if (p < 1)
                    {
                        pinch = false;
                        return;
                    }
                }
                else
                {
                    if (p < 1)
                        return;
                    pinch = true;
                    isFirst = true;
                }

                Leap.Vector lPos = h.PalmPosition;
                Vector3 pos = new Vector3(lPos.x, -lPos.z, lPos.y) / 10;
                //string msg = pos.ToString();
                Leap.LeapQuaternion lq = h.Rotation;
                Quaternion q = new Quaternion(lq.x, -lq.z, lq.y, lq.w);
                q = Quaternion.Normalize(q);
                //Console.WriteLine( Vector3.Transform(Vector3.UnitY, q) + Vector3.Transform(Vector3.UnitZ, q));

                if (isFirst)
                {
                    startPos = pos;
                    startQInv = Quaternion.Inverse(q);

                    refPos = cursor.Pos;
                    refQ = cursor.Dir;
                }

                cursor.Pos = refPos + (pos - startPos);
                cursor.Dir = q * startQInv * refQ;

                valid = false;
            }
        }

        protected override bool update(float dt, out ComData msg)
        {
            lock(this)
            {
                if (valid)
                {
                    msg = null;
                    return false;
                }

                msg = new ComData(new KeyFrame(cursor, Time.Now()));
                valid = true;
                return true;
            }
        }
    }
}
