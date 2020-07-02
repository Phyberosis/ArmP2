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
using device.Arm;

namespace Devices
{
    internal class DeviceArm : Device
    {
        ServoDriver driver;

        public DeviceArm() : base()
        {
            string cmd;

            driver = new ServoDriver();
            driver.Run();
            while (true)
            {
                cmd = Console.ReadLine();
                if (cmd.Equals("x")) break;

                if (cmd.Equals("r"))
                {
                    com.Send(ComData.RequestReset());
                    continue;
                }

                com.Send(new ComData(cmd));
            }

            EventBulletin.GetInstance().Notify(EventBulletin.Event.CLOSE, null, null);
            Environment.Exit(0);
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
                    driver.SetTarget(f.Cursor);
                    break;
                case "JSONableArray":
                    JSONableArray<float> a = new JSONableArray<float>(float.Parse);
                    if (!raw.TryParse(ref a))
                    {
                        Console.WriteLine("failed array");
                        return;
                    }

                    Angle[] angles = new Angle[6];
                    float[] rawF = a.getArray();
                    Array.Copy(Array.ConvertAll(rawF, (x)=> { return new Angle(x); }),
                        2, angles, 0, angles.Length);   // first 2 is time and gripper

                    //Console.WriteLine("array: {0}", string.Join<Angle>(", ", angles));
                    driver.SetTarget(angles);
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
