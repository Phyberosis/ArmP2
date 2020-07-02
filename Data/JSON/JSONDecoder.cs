using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.JSON
{
    public class JSONDecoder
    {
        private StringReader sr;
        //private string raw;

        public JSONDecoder(string raw)
        {
            //this.raw = raw;
            sr = new StringReader(raw);
            sr.ReadLine();
        }

        public delegate x Parser<x>(string raw);

        public T ParseNext<T>(Parser<T> p)
        {
            string line = sr.ReadLine();
            line = line.Substring(line.IndexOf(": ") + 2);
            line = line.EndsWith(",") ? line.Substring(0, line.Length - 1) : line.Trim();
            return p(line);
        }

        public void SkipLine()
        {
            sr.ReadLine();
        }

        public JSONDecoder Clone()
        {
            string rest = sr.ReadToEnd();
            sr = new StringReader(rest);
            return new JSONDecoder(rest);
        }

        //public string GetAll()
        //{
        //    return raw;
        //}
    }
}
