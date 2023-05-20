using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Contracts.Devices;
using Contracts.Media;

namespace Contracts.GroupingStuff
{
    public class GroupingDTO
    {
        public string Id { get; set; }

        public string? Name { get; set; }
        public string? Description { get; set; }
        public ICollection<DeviceDTO> Devices{ get; set; }
        public bool IsBaseGrouping { get; set; }


    }



    public class GroupingRegistrationRequest
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
    }

    public class GroupingUpdateRequest
    {

        [Required]

        public string Id { get; set; }
        [Required]
        public string? Name { get; set;}
        [Required]
        public string? Description { get; set;}

    }
}
