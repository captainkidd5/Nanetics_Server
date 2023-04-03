using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Authentication.Identity
{
    public class BearerTokenDTO
    {
        public string token { get; set; }
        public DateTime validTo { get; set; }
        public string email { get; set; }
    }
}
