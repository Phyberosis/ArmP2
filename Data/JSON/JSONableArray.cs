using Data.JSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace Data.JSON
{
    public class JSONableArray<T> : IJSONable
    {
        public delegate T Parser(string s);

        private T[] arr;
        private Parser parse;

        public JSONableArray(Parser p) : this(new T[0], p)
        { }

        public JSONableArray(T[] a, Parser p)
        {
            arr = a;
            parse = p;
        }

        public T[] getArray()
        {
            return arr;
        }

        public void addToJSON(JSONBuilder jb)
        {
            jb.addPrimitiveArray(typeof(T).Name, arr);
            jb.closeMe();
        }

        public void fillFromJSON(JSONDecoder jd)
        {
            jd.SkipLine();
            bool hasNext = true;
            List<T> arrBuilder = new List<T>();
            while(hasNext)
            {
                hasNext = jd.ParseNext((str) =>
                {
                    if (str.Contains(']')) return false;
                    int i = str.IndexOf(',');
                    T x = i != -1 ? parse(str.Substring(0, i)) : parse(str.Substring(0));
                    arrBuilder.Add(x);
                    return true;
                });
            }

            arr = arrBuilder.ToArray();
        }
    }
}
