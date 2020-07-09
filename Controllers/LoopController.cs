using Communications;
using Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Input
{
    internal abstract class LoopController : Controller
    {
        private Thread worker;
        private bool stop = false;

        protected LoopController(Com comObject) : base(comObject)
        {

        }

        protected override void begin()
        {
            stop = false;
            worker = new Thread(() =>
            {
                float last = Time.Now();
                while (true)
                {
                    lock(worker)
                    {
                        if (stop) break;
                    }

                    float now = Time.Now();
                    float dt = now - last;
                    if (dt < 0.05) continue;

                    ComData msg;
                    if(update(dt, out msg))
                        sendData(msg);

                    last = now;
                }
            });

            worker.Start();
        }

        protected override void end()
        {
            lock(worker)
            {
                stop = true;
            }
            worker.Join();
        }

        protected abstract bool update(float dt, out ComData msg);
    }
}
