using Devices.Arm;

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
