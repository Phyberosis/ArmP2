using Data.JSON;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Data.Arm
{
    public struct KeyFrame : IJSONable
    {
        public ArmCursor cursor;
        public float Time;

        public KeyFrame(Vector3 v, Quaternion q, float t) : this(new ArmCursor(v, q), t)
        {

        }

        public KeyFrame(ArmCursor c, float time)
        {
            cursor = c;
            Time = time;
        }

        public void addToJSON(JSONBuilder jb)
        {
            jb.addObject("ArmCursor", cursor);
            jb.addPrimitive("Time", Time);
            jb.closeMe();
        }

        public void fillFromJSON(JSONDecoder jd)
        {
            jd.SkipLine();

            cursor = new ArmCursor();
            cursor.fillFromJSON(jd);
            jd.SkipLine();
            Time = jd.ParseNext((str) =>
            {
                float f;
                float.TryParse(str, out f);
                return f;
            });
        }

        public override string ToString()
        {
            return cursor.ToString() + "\n@ " + Time.ToString() + "s";
        }
    }
}
