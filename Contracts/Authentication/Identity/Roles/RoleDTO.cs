using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.Authentication.Identity.Roles
{
    public class RoleDTO
    {
        public string Id { get; set; }
        [Required]
        [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "Only letters are allowed")]
        [MinLength(1,ErrorMessage ="Must be at least 1 character")]
        [MaxLength(20, ErrorMessage = "Exceeded maxmimum length of 20 characters")]
        public string? Name { get; set; }
    }

    public class RoleInfoQueryResponse
    {
        public string Name { get; set; }
        public List<string> Users { get; set; }
    }
}
