using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.Devices.IoT
{
    public class IotCollection
    {
        public string nextLink { get; set; }
        public object[] value { get; set; }
    }
}
