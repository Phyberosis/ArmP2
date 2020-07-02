using Data;
using Data.JSON;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Communications
{
    public class Packet
    {
        public readonly PacketKey Key;
        public readonly ComData Data;

        public Packet(ComData d)
        {
            Key = new PacketKey();
            Data = d;
        }

        public static bool Unpack(string json, out Packet p)
        {
            p = null;

            if (json.Equals("")) return false;

            try
            {
                p = new Packet(json);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
            return true;
        }

        private Packet(string json)
        {
            JSONDecoder jd = new JSONDecoder(json);

            Key = new PacketKey();
            Key.fillFromJSON(jd);

            Data = new ComData("");
            jd.SkipLine();
            Data.fillFromJSON(jd);
        }

        public string Pack()
        {
            JSONBuilder jb = new JSONBuilder();
            Key.addToJSON(jb);
            jb.addObject("Data", Data);
            jb.closeMe();
            return jb.ToString();
        }
    }
}
