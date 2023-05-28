using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.Devices.IoT.Visuals
{

    /// <summary>
    /// https://learn.microsoft.com/en-us/rest/api/iotcentral/2022-10-31-previewdataplane/dashboards/list?tabs=HTTP#tile
    /// </summary>
    public class IoTTile
    {
        public object configuration { get; set; }
        public string displayName { get; set; }
        public int height { get; set; }
        public string id { get; set; }
        public int width { get; set; }
        public int x { get; set; }
        public int y { get; set; }
    }

    public class TileCapability
    {

    }

    
}
