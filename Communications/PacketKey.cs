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
    public class PacketKey : IJSONable
    {
        private long value;

        public PacketKey(PacketKey other)
        {
            value = other.value;
        }

        public PacketKey(long val)
        {
            value = val;
        }

        public PacketKey()
        {
            value = Stopwatch.GetTimestamp();
        }

        public long getValue()
        {
            return value;
        }

        public void addToJSON(JSONBuilder jb)
        {
            jb.addPrimitive("Key", value);
        }

        public void fillFromJSON(JSONDecoder jd)
        {
            value = jd.ParseNext(long.Parse);
        }

        public override string ToString()
        {
            return value.ToString();
        }

        public override bool Equals(object obj)
        {
            if(obj.GetType() == typeof(PacketKey))
            {
                return (PacketKey)obj == this;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return (int)(value % int.MaxValue);
        }

        public static bool operator <=(PacketKey me, PacketKey other)
        {
            if (me is null || other is null) return false;
            return me.value <= other.value;

        }

        public static bool operator >=(PacketKey me, PacketKey other)
        {
            if (me is null || other is null) return false;
            return me.value >= other.value;

        }

        public static bool operator >(PacketKey me, PacketKey other)
        {
            if (me is null || other is null) return true;
            return me.value > other.value;
        }

        public static bool operator <(PacketKey me, PacketKey other)
        {
            if (me is null || other is null) return true;
            return me.value < other.value;
        }

        public static bool operator ==(PacketKey me, PacketKey other)
        {
            if (me is null || other is null) return false;
            return me.value.Equals(other.value);
        }

        public static bool operator !=(PacketKey me, PacketKey other)
        {
            if (me is null || other is null) return true;
            return !me.value.Equals(other.value);
        }
    }
}
