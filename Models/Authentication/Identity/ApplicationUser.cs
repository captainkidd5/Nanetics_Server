

using Microsoft.AspNetCore.Identity;
using Models.BusinessStuff;
using Models.Devices;
using Models.Phones;

namespace Models.Authentication
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public string? RefreshToken { get; set; }

        public string ConfirmationCode { get; set; }

        public DateTime RefreshTokenExpiryTime { get; set; }



        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        public List<Business> Businesses { get; set; }

        public List<Device> Devices { get; set; }


    }
}