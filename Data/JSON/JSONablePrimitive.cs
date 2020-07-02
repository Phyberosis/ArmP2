using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace Data.JSON
{
    public class JSONablePrimitive<T>
    {
        private readonly T[] vals;
        private readonly string name;

        public string ToJSON()
        {
            JSONBuilder jb = new JSONBuilder();
            if (vals.Length == 1)
                jb.addPrimitive(name, vals[0]);
            else
                jb.addPrimitiveArray(name, vals);

            return jb.ToString();
        }
    }
}
