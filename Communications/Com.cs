using Data;
using Data.JSON;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net.Mail;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Communications
{
    // todo - gracefully handle reconnections
    public abstract class Com
    {
        private const bool VERBOSE = false;

        protected const int WAIT_INTERVAL = 100;
        protected bool running = false;

        PacketKey lastPacketKey;
        private Thread writeThread;
        private AutoResetEvent writeResetEvent;

        private LinkedList<Packet> sendQ;

        public delegate void onReadDataDelegate(ComData data);
        private onReadDataDelegate onReadData = null;
        protected delegate void incommingReadDelegate(string msg);

        //public Action onCloseDelegate = null;

        public Com()
        {
            init();
        }

        private void init()
        {
            lastPacketKey = new PacketKey(0); // is time

            writeThread = new Thread(writeLoop);

            sendQ = new LinkedList<Packet>();
            writeResetEvent = new AutoResetEvent(false);

            EventBulletin.Subscribe(EventBulletin.Event.CLOSE, (o, e) => { Close(); });

            Task.Delay(0).ContinueWith((t) =>
            {
                connectProcedure();
                running = true;

                beginRead(incommingRead);
                writeThread.Start();
            });
        }

        public void Send(ComData msg)
        {
            if (!syncRunning()) return;

            lock(sendQ)
            {
                sendQ.AddLast(new Packet(msg));

                //if (VERBOSE) Console.WriteLine("send queued <" + sendQ.Last.Value.Key + ">: " + msg.getDataType());
            }

            writeResetEvent.Set();
        }

        public Promise Request(ComData.Request req)
        {
            return new Promise();
        }

        public void setOnRead(onReadDataDelegate callback)
        {
            onReadData = callback;
        }

        protected abstract void connectProcedure();

        protected abstract void dispose();

        protected abstract void beginRead(incommingReadDelegate callback);

        protected void incommingRead(string msg)
        {
            Packet p;
            if (Packet.Unpack(msg, out p) && p.Key > lastPacketKey)
            {
                lastPacketKey = p.Key;
                if (VERBOSE) Console.WriteLine("Read <" + p.Key.getValue() + ">: " + p.Data.getDataType());

                //request?
                string dt = p.Data.getDataType();
                if (dt.Equals(ComData.REQUEST))
                {
                    switch(p.Data.GetRequest())
                    {
                        case ComData.Request.RESET: // todo - fix this
                            reset();
                            break;
                        case ComData.Request.CLOSE:
                            Close();
                            break;
                    }
                    return;
                }

                onReadData(p.Data);
            }

            beginRead(incommingRead);
        }

        protected abstract void write(string msg);

        private void writeLoop()
        {
            while(true)
            {
                if (!syncRunning())
                {
                    //Packet close = new Packet(ComData.RequestClose());
                    //write(close.Pack());
                    //Console.WriteLine("write close");
                    break;
                }

                Packet msg = null;
                lock(sendQ)
                {
                    if(sendQ.Count > 0)
                    {
                        msg = sendQ.First();
                        sendQ.RemoveFirst();
                    }
                }

                if (msg != null)
                {
                    string toSend = msg.Pack();
                    write(toSend);   // may block on this

                    if (VERBOSE) Console.WriteLine("sent <" + msg.Key + ">\n" + toSend + "\n");
                }
                else
                {
                    writeResetEvent.WaitOne();
                }
            }
        }

        private bool syncRunning()
        {
            lock(writeThread)
            {
                return running;
            }
        }

        public void Close()
        {
            lock (writeThread)
            {
                if (!running)
                {
                    dispose();
                    return;
                }

                running = false;
            }
            Console.WriteLine("Com close - pre");

            writeResetEvent.Set();
            try
            {
                writeThread.Join();
            }
            catch (ThreadStateException)
            { }

            dispose();

            Console.WriteLine("Com close");
        }

        public void reset()
        {
            Close();
            init();
        }
    }
}