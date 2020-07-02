using Data;
using Data.JSON;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Communications
{
    public class Test
    {

        public void run()
        {
            // packet keys
            PacketKey k = new PacketKey();
            JSONBuilder kjb = new JSONBuilder();
            kjb.closeMe();
            k.addToJSON(kjb);

            Console.WriteLine(kjb.ToString());
            Console.WriteLine("====");

            // packets
            Packet p = new Packet(new ComData("test"));
            string sp = p.Pack();
            Console.WriteLine(sp);

            Packet p2;
            Packet.Unpack(sp, out p2);

            Console.WriteLine(p2);
        }
    }
}
