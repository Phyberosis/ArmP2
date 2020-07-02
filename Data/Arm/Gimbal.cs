using Data.JSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Data.Arm
{
    // deprecated
    internal struct Gimbal : IJSONable
    {
        private Vector3 vec;
        private Angle mag;

        //public Gimbal(Vector3 vector, Angle magnitude)
        //{
        //    vec = vector;
        //    mag = magnitude;
        //}

        //public void Rotate(Vector3 vector, Angle magnitude)
        //{
        //    if (vec == vector)
        //    {
        //        mag += magnitude;
        //        return;
        //    }
        //}

        public override string ToString()
        {
            return "Vector   : " + vec.ToString() + "\nMagnitude: " + mag.ToString();
        }

        public void addToJSON(JSONBuilder jb)
        {
            jb.addPrimitive("Vec", vec);
            jb.addPrimitive("Mag", mag);
            jb.closeMe();
        }

        public void fillFromJSON(JSONDecoder jd)
        {
            throw new NotImplementedException();
        }
    }
}
