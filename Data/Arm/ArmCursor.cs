using Data.JSON;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Data.Arm
{
    public struct ArmCursor : IJSONable
    {
        public Vector3 Pos;
        public Quaternion Dir;

        public ArmCursor(Vector3 pos, Quaternion dir)
        {
            Pos = pos;
            Dir = dir;
        }

        public void Set(Vector3 pos, Quaternion dir)
        {
            Pos = pos;
            Dir = dir;
        }

        public void addToJSON(JSONBuilder jb)
        {
            jb.addPrimitive("Pos", Pos);
            jb.addPrimitive("Dir", Dir);
            jb.closeMe();
        }

        public void fillFromJSON(JSONDecoder jd)
        {
            Pos = jd.ParseNext((str) => 
            {
                float[] ax = new float[3];
                int a = 1, b;
                string[] c = { ", ", ", ", ">" };
                for (int i = 0; i < 3; i++)
                {
                    b = str.IndexOf(c[i], a);
                    ax[i] = float.Parse(str.Substring(a, b - a));
                    a = b + 2;
                }

                return new Vector3(ax[0], ax[1], ax[2]);
            });

            Dir = jd.ParseNext((str) =>
            {
                float[] ax = new float[4];
                int a = 3, b;
                string[] c = { " Y:", " Z:", " W:", "}" };
                for (int i = 0; i < 3; i++)
                {
                    b = str.IndexOf(c[i], a);
                    ax[i] = float.Parse(str.Substring(a, b - a));
                    a = b + 3;
                }

                return new Quaternion(ax[0], ax[1], ax[2], ax[3]);
            });
        }

        public override string ToString()
        {
            return "Pos: " + Pos.ToString() + "\n" + Dir.ToString();
        }

        public static ArmCursor operator +(ArmCursor me, Vector3 other)
        {
            return new ArmCursor(me.Pos + other, me.Dir);
        }
    }
}
