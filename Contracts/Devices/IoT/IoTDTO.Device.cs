using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.Devices.IoT
{

    public class IoTDeviceDTO
    {
        public DeploymentManifest deploymentManifest { get; set; }
        public string displayName { get; set; }
        public bool enabled { get; set; }
        public string eTag { get; set; }
        public string id { get; set; }
        public string[] organizations { get; set; }
        public bool provisioned { get; set; }
        public bool simulated { get; set; }
        public string template { get; set; }
        public string[] type { get; set; }
    }

    public class DeploymentManifest
    {
        public object data { get; set; }
        public string displayName { get; set; }
        public string etag { get; set; }
        public string id { get; set; }
        public string[] organizations { get; set; }
    }

    public class UpdateIoTDeviceRequest
    {
        public string displayName { get; set; }
    }


}
