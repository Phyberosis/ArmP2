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
        public ArmCursor Cursor;
        public float Time;

        public KeyFrame(Vector3 v, Quaternion q, float t) : this(new ArmCursor(v, q), t)
        {

        }

        public KeyFrame(ArmCursor c, float time)
        {
            Cursor = c;
            Time = time;
        }

        public void addToJSON(JSONBuilder jb)
        {
            jb.addObject("ArmCursor", Cursor);
            jb.addPrimitive("Time", Time);
            jb.closeMe();
        }

        public void fillFromJSON(JSONDecoder jd)
        {
            jd.SkipLine();

            Cursor = new ArmCursor();
            Cursor.fillFromJSON(jd);
            jd.SkipLine();
            Time = jd.ParseNext((str) =>
            {
                float f;
                float.TryParse(str, out f);
                return f;
            });
        }
    }
}
