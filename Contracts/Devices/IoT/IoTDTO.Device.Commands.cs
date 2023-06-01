using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.Devices.IoT
{
    public class IoTCommandDTO
    {
        public int connectionTimeout { get; set; }
        public string id { get; set; }
        public object request { get; set; }
        public object response { get; set; }
        public int responseCode { get; set; }
        public int responseTimeout { get; set; }
    }
}
