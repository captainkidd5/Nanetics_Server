using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.Devices.IoT
{

    public class IoTDeviceDTO
    {
        public DeploymentManifest DeploymentManifest { get; set; }
        public string DisplayName { get; set; }
        public bool Enabled { get; set; }
        public string ETag { get; set; }
        public string Id { get; set; }
        public string[] Organizations { get; set; }
        public bool Provisioned { get; set; }
        public bool Simulated { get; set; }
        public string Template { get; set; }
        public string[] Type { get; set; }
    }
    public class DeploymentManifest
    {
        public object Data { get; set; }
        public string DisplayName { get; set; }
        public string ETag { get; set; }
        public string Id { get; set; }
        public string[] Organizations { get; set; }
    }

}
