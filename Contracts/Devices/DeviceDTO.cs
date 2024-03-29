﻿using Contracts.Authentication.Identity.Create;
using Contracts.GroupingStuff;
using Microsoft.Azure.Devices;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.Devices
{

    public class DeviceDTO
    {
        public string Id { get; set; } = string.Empty;
        public string Nickname { get; set; } = string.Empty;

        public string HardwareId { get; set; }

        /// <summary>
        /// Basically the version, used for firmware updates
        /// </summary>
        public int GenerationId { get; set; }
        public string ETag { get; set; } = string.Empty;

        [JsonConverter(typeof(StringEnumConverter))]
        public DeviceConnectionState ConnectionState { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public DeviceStatus Status { get; set; }


        public DateTime ConnectionStateUpdatedTime { get; set; }

        public DateTime StatusUpdatedTime { get; set; }

        public DateTime LastActivityTime { get; set; }

        public int CloudToDeviceMessageCount { get; set; }

        public string GroupingId{ get; set; }

        public string UserId { get; set; }
    }
    public class PingDTO
    {
        public string Message { get; set; }
    }

    public class DeviceRegistryRequest
    {
        public string DeviceHardWareId { get; set; }
        public string TemplateName { get; set; }
    }

    public class DeviceRegistryResponse
    {
        public string X509Thumbprint { get; set; }
        public string AssignedId { get; set; }
    }

    public class DeviceQueryResponse
    {
        public List<DeviceDTO> Devices { get; set; }
        public int TotalCount { get; set; }
    }

    public class DeviceUpdateRequest
    {

        [Required]
        public string Id { get; set; }
        [Required]
        public string? Nickname { get; set; }
        public string? GroupingId { get; set; }

    }

    public class DeviceUnregisterRequest
    {

        [Required]
        public string Id { get; set; }


    }
}
