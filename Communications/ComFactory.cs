using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Communications
{
    public class ComFactory
    {
        public static Com MakeDefault()
        {
            return new ComLAN();
        }

        public static Com MakeDummy()
        {
            return new ComDummy();
        }
    }
}
