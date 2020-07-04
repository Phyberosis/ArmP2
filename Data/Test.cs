using Data.Arm;
using Data.JSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Data
{
    public class Test
    {

        public bool Run()
        {
            // Cursor and Keyframes
            ArmCursor ac = new ArmCursor(Vector3.UnitX, Quaternion.Identity);
            KeyFrame kf = new KeyFrame(ac, (float)Math.PI);
            JSONBuilder jkf = new JSONBuilder();
            ComData cd1 = new ComData(kf);
            cd1.addToJSON(jkf);
            Console.WriteLine(jkf.ToString());

            ComData cd1b = new ComData("");
            cd1b.fillFromJSON(new JSONDecoder(jkf.ToString()));
            KeyFrame kf2 = new KeyFrame();
            Console.WriteLine(cd1b.TryParse(ref kf2));

            Angle[] a =
            {
                new Angle(0.1f),
                new Angle(0.2f),
                new Angle(0.3f),
                new Angle(0.4f)
            };
            JSONBuilder ja = new JSONBuilder();
            JSONableArray<Angle> aa = new JSONableArray<Angle>(a, (str) => { return new Angle(float.Parse(str)); });
            ComData cd2 = new ComData(aa);
            cd2.addToJSON(ja);
            Console.WriteLine(ja.ToString());

            ComData cd2b = new ComData("");
            cd2b.fillFromJSON(new JSONDecoder(ja.ToString()));
            JSONableArray<Angle> aa2 = new JSONableArray<Angle>((str) => { return new Angle(float.Parse(str)); });
            Console.WriteLine(cd2b.TryParse(ref aa2));

            return true;
        }
    }
}
