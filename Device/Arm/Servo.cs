using Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Unosquare.RaspberryIO;
using Unosquare.RaspberryIO.Abstractions;

namespace Devices.Arm
{
    public abstract class Servo
    {
        protected float circ;
        protected float curr = 0;
        protected int dStep = 1;
        protected bool GlobalRev;

        protected int max, min;
        protected Angle fMid;

        private const float PI = (float)Math.PI;

        public static Servo MakeStepper(ArmConfig config, int i)
        {
            return new Stepper(config, i);
        }

        public static Servo MakeTestServo(ArmConfig config, int i)
        {
            return new Test(config, i);
        }

        private Servo(ArmConfig config, int i)
        {
            circ = config.SERVO_SPECS[i, 0];

            float fMin = config.BOUNDS_RAD[i, 0];
            float fMax = config.BOUNDS_RAD[i, 1];

            if (fMax == fMin && fMax == 0)
            {
                Console.WriteLine("Servo init axis {0}, === no bounds ===", i+1);

                fMid = float.NaN;
                return; // no bounds
            }

            fMid = ((fMax + fMin) / 2f) + PI;

            if (fMin < 0) fMin += 2 * PI;
            if (fMax < 0) fMax += 2 * PI;
            max = (int)(fMax / 2f / PI * circ);
            min = (int)(fMin / 2f / PI * circ) + 1;

            Console.WriteLine("Servo init axis {0}, min {1}, max {2}, fMid {3}, circ {4}", i+1, min, max, fMid, circ);

        }

        public void Set(int i)
        {
            curr = i;
        }

        public void Set(Angle a)
        {
            curr += GetSteps(a);
            rectify();
        }

        public Angle GetAngle()
        {
            rectify();
            return PI * 2f * curr / circ;
        }

        public int GetSteps(Angle final)
        {
            Angle me = GetAngle();
            Angle d = final - me;
            int steps = (int)Math.Round(circ * (float)d / 2f / PI);
            if (!float.IsNaN((float)fMid) && 
                me.Intercepts(fMid, final)) steps -= steps < 0 ? -(int)circ : (int)circ;
            //Console.WriteLine("Servo GS, Int? {0}, fmid {1}, {2} to {3}", me.Intercepts(final, fMid), fMid, me, final);
            //Console.WriteLine((int)Math.Round(circ * (float)d / 2 / PI) + "<<");
            return steps;
        }

        private void rectify()
        {
            if (curr < 0) curr += circ;
            if (curr >= circ) curr -= circ;
        }

        public void SetReverse(bool rev)
        {
            if (!rev ^ dStep == 1)
            {
                dStep = rev ? -1 : 1;
                reverse(rev);
            }
        }

        protected abstract void reverse(bool rev);

        public void Step()
        {
            curr += dStep;
            bool ok = min < max ? curr < max && curr > min : curr < max || curr > min;
            //Console.WriteLine(ok);
            if (float.IsNaN((float)fMid) || ok) makeStep();
        }

        public abstract void Reset();

        protected abstract void makeStep();

        public int GetCycle()
        {
            return (int)circ;
        }

        public int GetPosition()
        {
            return (int)curr;
        }

        private class Stepper : Servo
        {
            private IGpioPin pin, dir;

            public Stepper(ArmConfig config, int i): base(config, i)
            {
                GlobalRev = config.SERVO_SPECS[i, 3] == 1;
                this.pin = Pi.Gpio[config.SERVO_SPECS[i, 1]];
                this.pin.PinMode = GpioPinDriveMode.Output;
                this.pin.Write(false);
                this.dir = Pi.Gpio[config.SERVO_SPECS[i, 2]];
                this.dir.PinMode = GpioPinDriveMode.Output;
                this.dir.Write(GlobalRev);
            }

            protected override void reverse(bool rev)
            {
                dir.Write(rev ^ GlobalRev);
                Thread.Sleep(1);
            }

            protected override void makeStep()
            {
                pin.Write(true);

                //Console.Write(curr);
                //if(rev) Console.Write("<"); else Console.Write(">");
            }

            public override void Reset()
            {
                pin.Write(false);
            }
        }

        private class Test : Servo
        {
            public Test(ArmConfig config, int i) : base(config, i)
            {

            }

            public override void Reset()
            {

            }

            protected override void makeStep()
            {
                //
            }

            protected override void reverse(bool rev)
            {
                //
            }
        }
    }
}
