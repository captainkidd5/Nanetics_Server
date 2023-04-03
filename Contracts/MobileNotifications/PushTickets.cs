using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.MobileNotifications
{
    [JsonObject(MemberSerialization.OptIn)]
    public class PushTicketRequest
    {
        [JsonProperty(PropertyName = "to")]
        public List<string> PushTo { get; set; }

        [JsonProperty(PropertyName = "data")]
        public object PushData { get; set; }

        [JsonProperty(PropertyName = "title")]
        public string PushTitle { get; set; }

        [JsonProperty(PropertyName = "body")]
        public string PushBody { get; set; }

        [JsonProperty(PropertyName = "ttl")]
        public int? PushTTL { get; set; }

        [JsonProperty(PropertyName = "expiration")]
        public int? PushExpiration { get; set; }

        [JsonProperty(PropertyName = "priority")]  //'default' | 'normal' | 'high'
        public string PushPriority { get; set; }

        [JsonProperty(PropertyName = "subtitle")]
        public string PushSubTitle { get; set; }

        [JsonProperty(PropertyName = "sound")] //'default' | null	
        public string PushSound { get; set; }

        [JsonProperty(PropertyName = "badge")]
        public int? PushBadgeCount { get; set; }

        [JsonProperty(PropertyName = "channelId")]
        public string PushChannelId { get; set; }

        [JsonProperty(PropertyName = "categoryId")]
        public string PushCategoryId { get; set; }
    }


    [JsonObject(MemberSerialization.OptIn)]
    public class PushTicketResponse
    {
        [JsonProperty(PropertyName = "data")]
        public List<PushTicketStatus> PushTicketStatuses { get; set; }

        [JsonProperty(PropertyName = "errors")]
        public List<PushTicketErrors> PushTicketErrors { get; set; }

    }

    [JsonObject(MemberSerialization.OptIn)]
    public class PushTicketStatus
    {

        [JsonProperty(PropertyName = "status")] //"error" | "ok",
        public string TicketStatus { get; set; }

        [JsonProperty(PropertyName = "id")]
        public string TicketId { get; set; }

        [JsonProperty(PropertyName = "message")]
        public string TicketMessage { get; set; }

        [JsonProperty(PropertyName = "details")]
        public object TicketDetails { get; set; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class PushTicketErrors
    {
        [JsonProperty(PropertyName = "code")]
        public string ErrorCode { get; set; }

        [JsonProperty(PropertyName = "message")]
        public string ErrorMessage { get; set; }
    }
}
