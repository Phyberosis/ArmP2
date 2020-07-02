using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devices
{
    public class DeviceFactory
    {
        public static Device MakeArm()
        {
            return new DeviceArm();
        }
    }
}
