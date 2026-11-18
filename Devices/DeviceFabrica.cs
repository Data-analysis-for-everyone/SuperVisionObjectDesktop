using SuperVisionObjectDesktop.Network.DataClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperVisionObjectDesktop.Devices
{
    public class DeviceFabrica
    {

        /// <summary>
        /// При появлении новых устройств - обновить фабрику
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static IDevice? BuildDevice(DeviceConfig? config)
        {
            if (config == null)
                return null;

            if (config.DeviceType == "PulseOximeter")
            {
                if (config.SensorType == "MAX30105")
                    return new MAX30105(config);
            }

            return null;


        }
    }
}
