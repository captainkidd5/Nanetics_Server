using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.Devices.IoT.Telemetry
{
    public class IoTTelemetry
    {
        public string timestamp { get; set; }
        public object value { get; set; }
    }
}
