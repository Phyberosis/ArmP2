using Data.JSON;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Data
{
    public class ComData : IJSONable
    {
        public enum Request
        {
            POSE, CLOSE, RESET, NONE
        }

        public static ComData RequestClose()
        {
            return new ComData(Request.CLOSE);
        }

        public static ComData RequestReset()
        {
            return new ComData(Request.RESET);
        }

        public const string STRING = "String";
        public const string REQUEST = "Request";

        private const string PRIM = "#";
        private const string SUFFIX = " - ";
        private const string STR_FLAG = PRIM + STRING + SUFFIX;
        private const string REQ_FLAG = PRIM + REQUEST + SUFFIX;

        private string DataType;    // also holds data if type is string
        private IJSONable Value;
        private JSONDecoder RawValue = null;

        public ComData(string msg)
        {
            DataType = STR_FLAG + msg;
            Value = null;
        }

        public ComData(Request r)
        {
            DataType = REQ_FLAG + r.ToString();
            Value = null;
        }

        public ComData(IJSONable o)
        {
            string rawT = o.GetType().Name;
            DataType = rawT.Replace("`1", "");//rawT.Replace("`1[", "<").Replace("]", ">");
            Value = o;
        }

        public string getDataType()
        {
            int l = DataType.IndexOf(SUFFIX) - PRIM.Length;
            return DataType.StartsWith(PRIM) ? DataType.Substring(PRIM.Length, l) : DataType;
        }

        public string getMessage()
        {
            return DataType.StartsWith(STR_FLAG) ? DataType.Substring(STR_FLAG.Length) : "";
        }

        public Request GetRequest()
        {
            return DataType.StartsWith(REQ_FLAG) ? (Request)Enum.Parse(Request.NONE.GetType(), DataType.Substring(REQ_FLAG.Length)) : Request.NONE;
        }

        public void addToJSON(JSONBuilder jb)
        {
            jb.addString("DataType", DataType);
            if (Value == null)
                jb.addNull("Value");
            else
            {
                jb.addObject("Value", Value);
            }
            jb.closeMe();
        }

        public void fillFromJSON(JSONDecoder jd)
        {
            //jd.SkipLine();
            DataType = jd.ParseNext((s) => { return s.Substring(1, s.Length - 2); });
            if(DataType.StartsWith(PRIM))
            {
                Value = null;
                return;
            }
            else
            {
                RawValue = jd;
            }
        }

        public bool TryParse<T>(ref T container)
        {
            JSONDecoder jd = RawValue.Clone();
            try
            {
                IJSONable raw = (IJSONable)container;
                raw.fillFromJSON(jd);
                container = (T)raw;
                return true;
            }
            catch(Exception)
            {
                return false;
            }
        }

        //public bool TryParse<IJSONable>(out IJSONable[] t)
        //{
        //    t = null;
        //    return false;
        //}
    }
}
