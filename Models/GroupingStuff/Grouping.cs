using Models.Authentication;
using Models.Devices;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.GroupingStuff
{
    public class Grouping
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public string BannerImagePath { get; set; }


        public ApplicationUser User { get; set; }

        public List<Device> Devices { get; set; }

        /// <summary>
        /// Given to every user by default and represents an area where devices are initially automatically assigned to.
        /// User should not be able to edit or delete this grouping if true
        /// </summary>
        public bool IsBaseGrouping { get; set; }
    }

}
