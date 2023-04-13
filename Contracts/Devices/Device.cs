using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.Devices
{
    public class PingDTO
    {
        public string Message { get; set; }
    }

    public class DeviceRegistryRequest
    {
        public string DeviceHardWareId { get; set; }
    }

    public class DeviceRegistryResponse
    {
        public string X509Thumbprint { get; set; }
        public string AssignedId { get; set; }
    }
}
