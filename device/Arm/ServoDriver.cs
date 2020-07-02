using Communications;
using Data;
using Data.Arm;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Unosquare.RaspberryIO;

namespace device.Arm
{
    internal class ServoDriver
    {
        //private readonly Pose target;
        private PoseCalculator poseCalculator;
        private ArmConfig config;

        private Servo[] servos;

        private Thread worker = null;
        private bool running = false;
        private AutoResetEvent notifyWorkerEvent;

        private float currentProgress = 1;
        private int[] servoProgress = new int[6];
        private int[] servoToStep = new int[6];

        private const string SAVE_FILE = @"/home/phyberosis/armp2c/save.txt";

        //private delegate Angle GetSliceDelegate(Angle a);
        //private GetSliceDelegate getSlice;

        public ServoDriver()
        {
            config = new ArmConfig();
            poseCalculator = new PoseCalculator(config);
            notifyWorkerEvent = new AutoResetEvent(false);

            //for(int i = 0; i<6; i++)
            //{
            //    servoProgress[i] = 1;
            //    servoToStep[i] = 1;
            //}

            Pi.Init<Unosquare.WiringPi.BootstrapWiringPi>();
            servos = config.CreateSteppers();

            //servos = config.CreateTestServos();

            load();
        }

        public Angle[] Reset(ArmCursor orig)
        {
            Angle[] a = poseCalculator.PoseToAngles(orig.Pos, orig.Dir);
            for (int i = 0; i < a.Length; i++)
            {
                servos[i].Set(a[i]);
            }

            return a;
        }

        public Angle[] GetAngles()
        {
            Angle[] a = new Angle[servos.Length];
            for (int i = 0; i < servos.Length; i++)
            {
                a[i] = servos[i].GetAngle();
            }

            return a;
        }

        public Angle[] SetTarget(ArmCursor t)
        {
            //ArmCursor ac = poseCalculator.AnglesToPose(Array.ConvertAll(servos, (Servo s)=> { return s.GetAngle(); }));
            //Console.WriteLine("current cursor: " + ac);

            Angle[] angles = poseCalculator.PoseToAngles(t.Pos, t.Dir);
            //int i = 0;
            //foreach (Angle a in angles)
            //{
            //    Console.Write(a + " ");
            //    if (i == 2) Console.Write("|| ");
            //    i++;
            //}
            //Console.WriteLine();

            setTarget(angles);
            return angles;

            //ArmCursor ac = poseCalculator.AnglesToPose(angles);
            //Console.WriteLine("current cursor: " + ac);
        }

        public ArmCursor SetTarget(Angle[] t)
        {
            setTarget(t);
            return poseCalculator.AnglesToPose(t);
        }

        private void setTarget(Angle[] desired)
        {
            Console.WriteLine("st");
            for (int i = 0; i < servos.Length; i++)
            {
                servoToStep[i] = servos[i].GetSteps(desired[i]);
                //Console.WriteLine("sts"+servoToStep[i]);
                servos[i].SetReverse(servoToStep[i] < 0);
                servoToStep[i] = Math.Abs(servoToStep[i]);
                if (servoToStep[i] == 0)
                {
                    servoToStep[i] = 1;
                    servoProgress[i] = 1;
                }else
                {
                    servoProgress[i] = 0;
                }

                Console.Write(desired[i] + " ");
                if (i == 2) Console.Write("|| ");
            }
            Console.WriteLine();

            for(int i = 0; i < servos.Length; i++)
            {
                Console.Write(servoToStep[i] + " ");
            }
            Console.WriteLine();

            currentProgress = 0;

            notifyWorkerEvent.Set();
        }

        public void Run()
        {
            if (worker != null) return;
            running = true;

            worker = new Thread(() =>
            {
                while (true)
                {
                    lock(worker)
                    {
                        if (!running) break;
                    }

                    //Console.WriteLine("progress: "+currentProgress);
                    bool all = false;

                    for(int steps = 0; steps < 300 && currentProgress < 1; steps++)
                    {
                        float minP = 1;
                        for (int i = 0; i < servos.Length; i++)
                        {
                            float p = (float)servoProgress[i] / servoToStep[i];
                            //Console.WriteLine("p" + p + " " + Math.Min(minP, p) + " " + (i+1));

                            if (p > currentProgress) continue;      // this could be done better...

                            servos[i].Step();
                            servoProgress[i]++;
                            minP = Math.Min(minP, (float)servoProgress[i] / servoToStep[i]);
                            //Console.WriteLine("minP" + minP + " " + (i+1));

                            //if (steps == 0 || all) Console.Write(servoProgress[i] + " of " + servoToStep[i] + " " + (p * 100) + "% {" + (i + 1) + "} || ");
                        }
                        //if (steps == 0 || all) Console.WriteLine("minP "+minP);

                        const int dt = 500;
                        delay(dt);
                        foreach (Servo s in servos)
                        {
                            s.Reset();
                        }
                        delay(dt);

                        currentProgress = minP;
                        //if (steps == 0 || all || currentProgress == 1) Console.WriteLine((int)(currentProgress * 100) + "%");
                    }

                    if(currentProgress == 1)
                    {
                        notifyWorkerEvent.WaitOne();
                    }
                }
            });

            worker.Start();
        }

        private void delay(int us)
        {
            long start = Time.Micro();
            while(Time.Micro() - start < us)
            {
                // spin
            }
        }

        public void Close()
        {
            if (worker == null) return;
            lock (worker)
            {
                running = false;
            }

            notifyWorkerEvent.Set();
            notifyWorkerEvent.Close();

            //Console.WriteLine("servo driver Close - pre");
            worker.Join();
            worker = null;
            save();
            Console.WriteLine("servo driver Closed");
        }

        private void load()
        {
            StreamReader reader;
            try
            {
                reader = new StreamReader(SAVE_FILE);
            }catch(FileNotFoundException)
            {
                return;
            }

            reader.ReadLine();
            string line = reader.ReadLine();
            int axis = int.Parse(line.Substring(0, line.IndexOf(" ")));
            for(int i = 0; i<axis; i++)
            {
                line = reader.ReadLine();
                int start = 8;
                int l = line.IndexOf(" of ", start) - start;
                int step = int.Parse(line.Substring(start, l));
                //start += l + 4;
                //l = line.IndexOf(" ", start) - start;
                //int total = int.Parse(line.Substring(start, l));
                servos[i].Set(step);
            }

            reader.Close();
            Console.WriteLine("loaded");
        }

        private void save()
        {
            //string path = Path.GetDirectoryName(
            //         Assembly.GetAssembly(typeof(ServoDriver)).CodeBase);
            //Console.WriteLine(path);

            StreamWriter writer = new StreamWriter(SAVE_FILE, false, Encoding.UTF8);

            writer.WriteLine("Arm P2c - saved state");
            writer.WriteLine("{0} axis total", servos.Length);
            int i = 1;
            foreach (Servo s in servos)
            {
                writer.WriteLine("Axis {0}: {1} of {2} ", i, s.GetPosition(), s.GetCycle());
                i++;
            }

            writer.Flush();
            writer.Close();

            Console.WriteLine("saved");
        }
    }
}
