using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Communications
{
    internal class ComDummy : Com
    {
        private Thread later;
        private bool isRunning;
        protected override void beginRead(incommingReadDelegate callback)
        {
            later = new Thread(() =>
            {
                long i = 1;
                while (true)
                {
                    lock(later)
                    {
                        if (!isRunning) break;
                    }

                    callback("dummy read "+i);
                    i++;

                    try
                    {
                        Thread.Sleep(2000);
                    }
                    catch (ThreadInterruptedException) { }
                }
            });

            later.Start();
        }

        protected override void connectProcedure()
        {
            Console.WriteLine("Init: Dummy coms for testing");
        }

        protected override void dispose()
        {
            lock(later)
            {
                isRunning = false;
            }
            later.Interrupt();
            later.Join();
        }

        protected override void write(string msg)
        {
            Console.WriteLine("Dummy write:\n" + msg);
        }
    }
}
