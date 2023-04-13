using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Phone
{
    public class PhoneRegistrationRequest
    {
      
        public string Token { get; set; }

        //Use to see if should send daily notification to user
        public Calendar Calendar { get; set; }
    }

    public class Calendar
    {
        public string TimeZone { get; set; }
    }


    
}
