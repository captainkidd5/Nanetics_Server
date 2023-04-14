using Microsoft.Azure.Devices;
using Models.Authentication;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Models.Devices
{

 
    public class Device
    {
        public string Id { get; set; } = string.Empty;

        public string HardwareId { get; set; } = string.Empty;
        /// <summary>
        /// Basically the version, used for firmware updates
        /// </summary>
        public int GenerationId { get; set; }
        public string ETag { get; set; } = string.Empty;

        [JsonConverter(typeof(StringEnumConverter))]
        public DeviceConnectionState ConnectionState { get;  set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public DeviceStatus Status { get; set; }


        public DateTime ConnectionStateUpdatedTime { get;  set; }

        public DateTime StatusUpdatedTime { get;  set; }

        public DateTime LastActivityTime { get;  set; }

        public int CloudToDeviceMessageCount { get;  set; }

        [ForeignKey("User")]
        public Guid UserId { get; set; }
        public ApplicationUser User { get; set; }


    }
}
