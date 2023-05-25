using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.Devices.IoT
{

    public class IoTAPITokenResponseDTO
    {
        public string Expiry { get; set; }
        public string Id { get; set; }
        public RoleAssignmentDTO[] Roles { get; set; }
        public string Token { get; set; }
    }

    public class RoleAssignmentDTO
    {
        public string Organization { get; set; }
        public string Role { get; set; }
    }

    public class CreateDeviceRequestDTO
    {
        public string displayName { get; set; }
        public bool enabled { get; set; }
        public string etag { get; set; }
        public string[] organizations { get; set; }
        public bool simulated { get; set; }
        public string template { get; set; }
        public string[] type { get; set; }
    }

    public class IoTResponseError
    {
        public ErrorDetails error { get; set; }
    }

    public class ErrorDetails
    {
        public string code { get; set; }
        public string message { get; set; }
        public string requestId { get; set; }
        public string time { get; set; }
    }



    /// <summary>
    /// https://learn.microsoft.com/en-us/rest/api/iotcentral/2022-10-31-previewdataplane/device-groups/create?tabs=HTTP
    /// </summary>
    public class CreateGroupRequestDTO
    {
        public string DisplayName { get; set; }
        public string Filter { get; set; }
        public string Description { get; set; }
        public string ETag { get; set; }
        public string[] Organizations { get; set; }
    }

}
