using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Contracts.Media;

namespace Contracts.BusinessStuff
{
    public class BusinessDTO
    {
        public string Id { get; set; }

        public string? Name { get; set; }
        public string? Description { get; set; }

    }



    public class BusinessRegistrationRequest
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
    }

    public class BusinessUpdateRequest
    {

        [Required]

        public string Id { get; set; }
        [Required]
        public string? Name { get; set;}
        [Required]
        public string? Description { get; set;}

    }
}
