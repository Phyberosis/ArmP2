using Data;
using Data.Arm;
using Devices.Arm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Unosquare.RaspberryIO;
using Unosquare.RaspberryIO.Abstractions;

namespace Devices
{
    internal class Test
    {
        public static void Run()
        {
            //Quaternion qa = Quaternion.CreateFromAxisAngle(Vector3.UnitY, (float)Math.PI / 1.3f);
            //Quaternion qb = Quaternion.CreateFromAxisAngle(Vector3.UnitX, (float)Math.PI / -2.4f);

            //Quaternion q1 = Quaternion.CreateFromAxisAngle(Vector3.UnitY, (float)Math.PI / 2);
            //Quaternion q2 = Quaternion.CreateFromAxisAngle(Vector3.UnitX, (float)Math.PI / 2);

            //Quaternion qdiff = Quaternion.Inverse(q1) * q2;

            //Console.WriteLine("q1:  " + Vector3.Transform(Vector3.UnitZ, q1));
            //Console.WriteLine("-q1: " + Vector3.Transform(Vector3.UnitZ, Quaternion.Inverse(q1)));
            //Console.WriteLine("q2:  " + Vector3.Transform(Vector3.UnitZ, q2));
            //Console.WriteLine(" qd:  "+Vector3.Transform(-Vector3.UnitY, qdiff));

            //Angle a1 = (float)(2f * Math.Acos(qdiff.W));
            //Console.WriteLine(a1);

            //Angle a = 0;
            //Angle b = 1.5;
            //Angle i = 0.7;

            //Angle a2 = 3;
            //Angle b2 = -3;
            //Angle j = 3.1;

            //Console.WriteLine(a.Intercepts(b, i));
            //Console.WriteLine(a2.Intercepts(b2, j));

            ServoDriver sd = new ServoDriver();
            ArmCursor cursor = new ArmCursor();
            cursor.Dir = Quaternion.CreateFromAxisAngle(Vector3.UnitY, (float)Math.PI / 2f);
            cursor.Pos.X = 6f +      23;
            cursor.Pos.Z = -5.5f +   30.5f;
            ArmCursor orig = cursor;

            ArmCursor rest = new ArmCursor(new Vector3(17, 0, 6), cursor.Dir);
            Angle[] angles = sd.GetAngles();

            sd.Run();

            while (true)
            {
                //Console.Write("$");
                string s = Console.ReadLine();
                if (s.Length == 0) continue;
                if (s.Contains("x")) break;
                if (s.Contains("rr"))
                {
                    angles = sd.Reset(orig);
                    cursor = orig;
                    Console.WriteLine("synced angles");
                    continue;
                }

                if (s.Contains("cal"))
                {
                    angles = sd.SetTarget(orig);
                    continue;
                }

                if (s.Contains("rest"))
                {
                    angles = sd.SetTarget(rest);
                    continue;
                }

                if (s.Contains("p"))
                {
                    sd.SetTarget(angles);
                    continue;
                }

                if(s.Contains(" "))
                {
                    string[] ss = s.Split(' ');
                    int ax = int.Parse(ss[0].Trim());
                    float deg = float.Parse(ss[1].Trim());

                    angles[ax - 1] = deg * (float)Math.PI / 180f;
                    Console.WriteLine("axis " + ax + " to " + angles[ax - 1]);

                    cursor = sd.SetTarget(angles);
                }
                else
                {
                    s = s.ToUpper();
                    foreach (char ch in s)
                    {
                        t(ch, ref cursor);
                    }
                    Console.WriteLine("moving cursor to "+cursor.ToString());
                    angles = sd.SetTarget(cursor);
                }
            }

            sd.Close();
        }

        private static bool t(char key, ref ArmCursor target)
        {
            const float CTrans = 1;
            const float CRot = (float)Math.PI / 12;

            Vector3 dPos = Vector3.Zero;
            Vector3 dDir = Vector3.Zero;   // just for storing dRotation angles
            if (key == 'A')
                dPos.X += -1;
            if (key == 'D')
                dPos.X += 1;
            if (key == 'S')
                dPos.Y += -1;
            if (key == 'W')
                dPos.Y += 1;
            if (key == 'C')
                dPos.Z += -1;
            if (key == 'Z')
                dPos.Z += 1;

            // acting as temp rotation vector
            if (key == 'J')
                dDir.Y += -1;
            if (key == 'L')
                dDir.Y += 1;
            if (key == 'I')
                dDir.X += -1;
            if (key == 'K')
                dDir.X += 1;
            if (key == 'M')
                dDir.Z += -1;
            if (key == 'N')
                dDir.Z += 1;

            if (dPos == Vector3.Zero && dDir == Vector3.Zero)
            {
                return false;
            }

            dPos = dPos.LengthSquared() != 0 ? Vector3.Normalize(dPos) * CTrans : dPos;
            dDir = dDir.LengthSquared() != 0 ? Vector3.Normalize(dDir) * CRot : dDir;

            Quaternion q = Quaternion.CreateFromYawPitchRoll(dDir.Y, dDir.X, dDir.Z);
            q *= target.Dir;
            target.Set(target.Pos + dPos, q);

            return true;
        }

        //public static void Run()
        //{
        //    ServoDriver sd = new ServoDriver();
        //    sd.Run();

        //    float f = (float)(15f * Math.PI / 180f);

        //    Angle[] a = new Angle[] { 0, 0, 0, 0, 0, 0 };
        //    //Angle[] a = new Angle[] { f, f, f, f, f, f };
        //    //Angle[] a = new Angle[] { -f, -f, -f, -f, -f, -f };
        //    //Angle[] a = new Angle[] { 0, f, 2*f, 3*f, 4*f, 5*f };
        //    //sd.SetTarget(a);


        //    while (true)
        //    {
        //        Console.WriteLine("ready");
        //        string s = Console.ReadLine();
        //        if (s.Contains("x")) break;

        //        string[] ss = s.Split(' ');
        //        int ax = int.Parse(ss[0].Trim());
        //        float deg = float.Parse(ss[1].Trim());

        //        a[ax - 1] = deg * (float)Math.PI / 180f;
        //        Console.WriteLine("axis " + ax + " to " + a[ax - 1]);

        //        sd.SetTarget(a);
        //    }


        //    sd.Close();
        //}

        //public static void Run()
        //{
        //    Pi.Init<Unosquare.WiringPi.BootstrapWiringPi>();

        //    var led = Pi.Gpio[10];
        //    led.PinMode = GpioPinDriveMode.Output;

        //    var on = false;
        //    for (int i = 0; i < 10; i++)
        //    {
        //        on = !on;
        //        led.Write(on);
        //        Thread.Sleep(300);
        //    }
        //}
    }
}
