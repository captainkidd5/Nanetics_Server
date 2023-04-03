
using AutoMapper;
using Contracts.Authentication.Identity.Create;
using Contracts.Authentication.Identity.Roles;
using Contracts.BusinessStuff;
using Contracts.Logging;
using Contracts.Media;
using Models.Authentication;
using Models.BusinessStuff;
using Models.Logging;
namespace Models
{

    //call from respository (uppermost) folder
    //dotnet ef migrations add remove_cloudflare_api -s SilverMenu -p databaseservices
    //dotnet ef database update -s SilverMenu -p databaseservices
    public class MapperInitializer :Profile
    {
        public MapperInitializer()
        {
            CreateApplicationUserMaps();
        }

        private void CreateApplicationUserMaps()
        {

            CreateMap<ApplicationUser, UserDTO>().ReverseMap();
            CreateMap<ApplicationRole, RoleDTO>().ReverseMap();

            CreateMap<Log, LogDTO>().ReverseMap();

            CreateMap<ApplicationUser, CreateUserDTO>().ReverseMap();

            CreateMap<Business, BusinessDTO>().ReverseMap();
            CreateMap<Business, BusinessRegistrationRequest>().ReverseMap();








        }
    }
}
