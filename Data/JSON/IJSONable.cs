using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.JSON
{
    public interface IJSONable
    {
        void addToJSON(JSONBuilder jb);

        void fillFromJSON(JSONDecoder jd);
    }
}
