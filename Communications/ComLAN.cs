using Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Communications
{
    internal class ComLAN : Com
    {
        private struct LANSettings
        {
            private static readonly string IP_STRING_HOST = "10.0.0.225";
            private static readonly string IP_STRING_CLIENT = "10.0.0.100";
            private static readonly int PORT = 8080;
            public static readonly IPEndPoint IP_HOST = new IPEndPoint(IPAddress.Parse(IP_STRING_HOST), PORT);
            public static readonly IPEndPoint IP_CLIENT = new IPEndPoint(IPAddress.Parse(IP_STRING_CLIENT), PORT);
        }

        private Socket connection;
        private static readonly byte[] FLANK = new byte[]
        {
            0, 2, 4, 8, 16, 32, 64, 128,
        };
        private static readonly int CHUNK_SIZE = FLANK.Length;

        // todo - this is garbage >> send close message instead
        private readonly string[] acceptedSocketExceptions = {
            "An established connection was aborted by the software in your host machine",
            "A request to send or receive data was disallowed because the socket is not connected",
            "An existing connection was forcibly closed by the remote host",
            "Cannot access a disposed object",
            "Connection reset by peer",
            "The socket has been shut down"
        };

        public ComLAN() : base()
        { }

        protected override void connectProcedure()
        {
            IPEndPoint[] IPs = new IPEndPoint[]
            {
                LANSettings.IP_HOST,
                LANSettings.IP_CLIENT
            };

            string[] connectErrors = new string[]
            {
                "Only one usage of each socket address (protocol/network address/port) is normally permitted",
                "The requested address is not valid in this context"
            };

            foreach (IPEndPoint ip in IPs)
            {
                Console.WriteLine("Init at " + ip.ToString());
                Socket server = new Socket(AddressFamily.InterNetwork,
                                SocketType.Stream, ProtocolType.Tcp);
                try
                {
                    server.Bind(ip);
                    server.Listen(3);
                    Console.WriteLine("Started as server, waiting for device client");

                    connection = server.Accept();
                    Console.WriteLine("Client connected");
                }
                catch (SocketException e)
                {
                    if (e.Message.Contains(connectErrors[0]) || e.Message.Contains(connectErrors[1]))
                    {
                        Console.WriteLine("Port occupied, started as client");
                        server.Connect(ip);
                        Console.WriteLine("Connected to server");

                        connection = server;
                    }
                    else
                    {
                        Console.WriteLine(e.Message);
                        continue;
                    }
                }
                break;
            }
        }

        protected override void write(string msg)
        {
            SocketExceptionContainer(() =>
            {
                sendOp(msg);
            });
        }
        
        private struct ReadAcc
        {
            public int fLoc;
            public bool inBody, wasInBody;
            public LinkedList<char> msg;

            public ReadAcc(int fl, bool ib, bool wib, LinkedList<char> m)
            {
                fLoc = fl;
                inBody = ib;
                wasInBody = wib;
                msg = m;
            }
        }

        protected override void beginRead(incommingReadDelegate callback)
        {
            byte[] buffer = new byte[CHUNK_SIZE];
            ReadAcc acc = new ReadAcc(0, false, false, new LinkedList<char>());
            SocketExceptionContainer(() =>
            {
                connection.BeginReceive(buffer, 0, CHUNK_SIZE, SocketFlags.None,
                    (IAsyncResult ar) => { onReadOp(buffer, acc, callback); }, null);
            });
        }

        private void onReadOp(byte[] buffer, ReadAcc a, incommingReadDelegate callback)
        {
            a.wasInBody = a.wasInBody || a.inBody;

            foreach (byte b in buffer)
            {
                if (a.inBody)
                {
                    a.msg.AddLast(Convert.ToChar(b));

                    a.fLoc = b == FLANK[a.fLoc - 1] ? a.fLoc - 1 : CHUNK_SIZE;
                    a.inBody = a.fLoc != 0;
                }
                else
                {
                    a.fLoc = b == FLANK[a.fLoc] ? a.fLoc + 1 : 0;
                    a.inBody = a.fLoc == CHUNK_SIZE;
                }
            }

            if (!a.wasInBody || a.inBody)
            {
                SocketExceptionContainer(() =>
                {
                    connection.BeginReceive(buffer, 0, CHUNK_SIZE, SocketFlags.None,
                        (IAsyncResult ar) => { onReadOp(buffer, a, callback); }, null);
                });
            }
            else
            {
                for (int i = 0; i < CHUNK_SIZE; i++) a.msg.RemoveLast();
                string ret = new string(a.msg.ToArray()).Trim();

                callback(ret);
            }
        }

        private void sendOp(string msg)
        {
            //Console.WriteLine("sendop\n{0}", msg);

            byte[] f = new byte[CHUNK_SIZE];
            Array.Copy(FLANK, f, CHUNK_SIZE);
            connection.Send(f);

            char[] rawM = msg.ToCharArray();
            int i = 0;
            int chunks = rawM.Length / CHUNK_SIZE;
            byte[] chunk = new byte[CHUNK_SIZE];
            for (int ch = 0; ch < chunks; ch++)
            {
                for (int j = 0; j < CHUNK_SIZE; j++)
                {
                    chunk[j] = Convert.ToByte(rawM[i]);
                    i++;
                }
                connection.Send(chunk);
            }
            for (int j = 0; j < CHUNK_SIZE; j++)
            {
                if (i < rawM.Length)
                {
                    chunk[j] = Convert.ToByte(rawM[i]);
                    i++;
                }
                else
                {
                    chunk[j] = Convert.ToByte(' ');
                    i++;
                }
            }
            connection.Send(chunk);

            Array.Reverse(f);
            connection.Send(f);

            //Console.WriteLine("sendop complete");
        }

        private void SocketExceptionContainer(Action fn)
        {
            try
            {
                fn();
            }
            catch (Exception e)
            {
                foreach (string m in acceptedSocketExceptions)
                {
                    if (e.Message.Contains(m))
                    {
                        Console.WriteLine("Connection terminated");
                        EventBulletin.GetInstance().Notify(EventBulletin.Event.CLOSE, null, null);
                        return;
                    }
                }

                throw e;
            }
        }

        protected override void dispose()
        {
            connection.Close();
        }
    }
}
