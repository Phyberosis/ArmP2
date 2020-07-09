using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Communications;
using Data;
using Data.Arm;
using Data.JSON;
using Devices.Arm;

namespace Devices.Arm
{
    internal class DeviceArm : Device
    {
        ServoDriver driver;
        ArmCursor cursor;
        Angle[] angles;

        public DeviceArm() : base()
        {
            driver = new ServoDriver();
            cursor = new ArmCursor();
            cursor.Dir = Quaternion.CreateFromAxisAngle(Vector3.UnitY, (float)Math.PI / 2f);
            cursor.Pos.X = 6f + 23;
            cursor.Pos.Z = -5.5f + 30.5f;
            ArmCursor orig = cursor;

            ArmCursor rest = new ArmCursor(new Vector3(16, 0, 8), cursor.Dir);
            angles = driver.GetAngles();

            driver.Run();

            while (true)
            {
                //Console.Write("$");
                string s = Console.ReadLine();
                if (s.Length == 0) continue;
                if (s.Contains("x")) break;
                if (s.Contains("rr"))
                {
                    angles = driver.Reset(orig);
                    cursor = orig;
                    Console.WriteLine("synced angles");
                    continue;
                }

                if (s.Contains("cal"))
                {
                    cursor = orig;
                    angles = driver.SetTarget(orig);
                    continue;
                }

                if (s.Contains("rest"))
                {
                    cursor = rest;
                    angles = driver.SetTarget(rest);
                    continue;
                }

                if (s.Contains("p"))
                {
                    driver.SetTarget(angles);
                    continue;
                }

                if (s.Contains(" "))
                {
                    string[] ss = s.Split(' ');
                    int ax = int.Parse(ss[0].Trim());
                    float deg = float.Parse(ss[1].Trim());

                    angles[ax - 1] = deg * (float)Math.PI / 180f;
                    Console.WriteLine("axis " + ax + " to " + angles[ax - 1]);

                    cursor = driver.SetTarget(angles);
                }
                else
                {
                    s = s.ToUpper();
                    foreach (char ch in s)
                    {
                        processKeys(ch, ref cursor);
                    }
                    Console.WriteLine("moving cursor to " + cursor.ToString());
                    angles = driver.SetTarget(cursor);
                }
            }

            driver.Close();

            EventBulletin.GetInstance().Notify(EventBulletin.Event.CLOSE, null, null);
            Environment.Exit(0);
        }

        private static bool processKeys(char key, ref ArmCursor target)
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

        protected override void onComRead(ComData raw)
        {
            switch (raw.getDataType())
            {
                case ComData.REQUEST:
                    Console.WriteLine("Requst: " + raw.GetRequest().ToString());
                    break;
                case ComData.STRING:
                    Console.WriteLine("String : " + raw.getMessage());
                    break;
                case "KeyFrame":
                    KeyFrame f = new KeyFrame();
                    if (!raw.TryParse(ref f))
                    {
                        Console.WriteLine("failed keyframe");
                        return;
                    }
                    cursor = f.cursor;
                    Quaternion q = f.cursor.Dir;
                    Console.WriteLine(q);
                    Console.WriteLine(Vector3.Transform(Vector3.UnitX, q));
                    Console.WriteLine(Vector3.Transform(Vector3.UnitY, q));
                    Console.WriteLine(Vector3.Transform(Vector3.UnitZ, q));
                    Console.WriteLine();
                    angles = driver.SetTarget(f.cursor);
                    break;
                case "JSONableArray":
                    Console.WriteLine("set angles: do locally");
                    //JSONableArray<float> a = new JSONableArray<float>(float.Parse);
                    //if (!raw.TryParse(ref a))
                    //{
                    //    Console.WriteLine("failed array");
                    //    return;
                    //}

                    //Angle[] anglesFinal = new Angle[6];
                    //float[] rawF = a.getArray();
                    //int i = 0;
                    //Array.Copy(Array.ConvertAll(rawF, (x) => { return new Angle(x); }), 
                    //    2, anglesFinal, 0, anglesFinal.Length);   // first 2 is time and gripper

                    ////Console.WriteLine("array: {0}", string.Join<Angle>(", ", angles));
                    //driver.SetTarget(angles);
                    break;
                default:
                    Console.WriteLine("defaulted with: " + raw.getDataType());
                    break;
            }
        }

        protected override void onClose()
        {
            driver.Close();
            Environment.Exit(0);
        }
    }
}
