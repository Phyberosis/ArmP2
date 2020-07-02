using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Data.JSON
{
    public class JSONBuilder
    {
        private StringBuilder content;
        private const string TAB = "    ";
        int tabCount = 0;

        public JSONBuilder()
        {
            content = new StringBuilder();
            content.Append("{\n");
            tabCount++;
        }

        private void applyTab()
        {
            for (int i = tabCount; i > 0; i--)
                content.Append(TAB);
        }

        private void addEntry(string name, string val)
        {
            applyTab();
            content.Append("\"" + name + "\": " + val);
        }

        public void addPrimitive<T>(string name, T val)
        {
            addEntry(name, val.ToString() + ",\n");
        }

        public void addPrimitiveArray<T>(string name, T[] vals)
        {
            addEntry(name, "[\n");
            tabCount++;

            for (int i = 0; i < vals.Length; i++)
            {
                T e = vals[i];
                applyTab();
                if(i != vals.Length - 1)
                    content.Append(e).Append(",\n");
                else
                    content.Append(vals[i]).Append("\n");
            }

            tabCount--;
            applyTab();
            content.Append("],\n");
        }

        public void addString (string name, string val)
        {
            addEntry(name, "\"" + val + "\",\n");
        }

        public void addObject(string name, IJSONable val)
        {
            addEntry(name, "{\n");
            tabCount++;
            val.addToJSON(this);
        }
        public void closeMe()
        {
            content.Remove(content.Length - 2, 1); // the last comma

            tabCount--;
            applyTab();
            content.Append("},\n");
        }

        //public void addArrPrim<T>(string name, T[] vals)
        //{
        //    applyTab();
        //    content.Append("\"" + name + "\": [\n");
        //    tabCount++;

        //    foreach(T v in vals)
        //    {
        //        applyTab();
        //        content.Append()
        //    }
        //}

        public void addArrObj(string name, IJSONable[] vals)
        {
            addEntry(name, "[\n");
            tabCount++;
            
            foreach(IJSONable o in vals)
            {
                applyTab(); content.Append("{\n");
                o.addToJSON(this);
                content.Insert(content.Length - 2, ",", 1);
            }

            content.Remove(content.Length - 2, 1); // the last comma
            tabCount--;
            applyTab(); content.Append("],\n");
        }

        public void addNull(string name)
        {
            addEntry(name, "null,\n");
        }

        public override string ToString()
        {
            return content.ToString().Substring(0, content.Length - 2);
        }
    }
}
