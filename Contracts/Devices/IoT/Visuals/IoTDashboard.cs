using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.Devices.IoT.Visuals
{
    /// <summary>
    /// https://learn.microsoft.com/en-us/rest/api/iotcentral/2022-10-31-previewdataplane/dashboards/list?tabs=HTTP#dashboardcollection
    /// </summary>
    public class DashboardCollection
    {
        public string nextLink { get; set; }
        public IoTDashboard[] value { get; set; }
    }

    public class IoTDashboard
    {
        public string displayName { get; set; }
        public string etag { get; set; }
        public bool favorite { get; set; }
        public string id { get; set; }
        public string[] organizations { get; set; }
        public bool personal { get; set; }
        public IoTTile[] tiles { get; set; }
    }
}
