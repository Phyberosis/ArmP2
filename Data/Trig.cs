using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Data
{
    public class Trig
    {
        // project raw onto target
        public static Vector3 Project(Vector3 raw, Vector3 target)
        {
            Vector3 dir = Vector3.Normalize(target);
            double l = Vector3.Dot(raw, dir);
            Vector3 result = Vector3.Multiply(dir, (float)l);

            return result;
        }

        // squash raw onto plain
        public static Vector3 Squash(Vector3 raw, Vector3 normal)
        {
            Vector3 difference = Project(raw, normal);
            Vector3 result = Vector3.Subtract(raw, difference);

            return result;
        }

        // get angle between a and b
        public static Angle AngleBetween(Vector3 a, Vector3 b)
        {
            double top = Vector3.Dot(a, b);
            double bot = a.Length() * b.Length();
            double raw = Math.Acos(top / bot);

            return (Angle)raw;
        }

        public static Angle AngleBetween(Quaternion a, Quaternion b)
        {
            double top = Quaternion.Dot(a, b);
            double bot = a.Length() * b.Length();
            double raw = Math.Acos(top / bot);

            return (Angle)raw;
        }
    }
}
