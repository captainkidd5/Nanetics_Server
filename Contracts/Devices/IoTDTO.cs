using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.Devices
{
    
    public class IoTAPITokenResponseDTO
    {
        public string Expiry { get; set; }
        public string Id { get; set; }
        public RoleAssignmentDTO[] Roles{ get; set; }
        public string Token  { get; set; }
    }

    public class RoleAssignmentDTO
    {
        public string Organization { get; set; }
        public string Role { get; set; }
    }

    public class CreateDeviceRequestDTO
    {
        public string DisplayName { get; set; }
        public bool Enabled { get; set; }
        public string ETag { get; set; }
        public string[] Organizations{ get; set; }
        public bool Simulated { get; set; }
        public string Template { get; set; }
        public string[] Type { get; set; }
    }
}
