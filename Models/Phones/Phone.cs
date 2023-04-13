using Models.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Phones
{
    public class Phone
    {
        public string Id { get; set; }

        /// <summary>
        /// Token generated for a specific mobile device whenever they accept push notifications. This is reset once the
        /// app is is reloaded, so this should be cleared whenever a new build is released
        /// </summary>
        public string ExpoPushToken { get; set; }

        //Use to see if should send daily notification to user
        public string TimeZone { get; set; }

        public string UserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }

    }
}
