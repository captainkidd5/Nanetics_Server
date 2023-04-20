
using AutoMapper;
using Contracts.Authentication.Identity.Create;
using Contracts.Authentication.Identity.Roles;
using Contracts.Devices;
using Contracts.GroupingStuff;
using Contracts.Media;
using Models.Authentication;
using Models.Devices;
using Models.GroupingStuff;
namespace Models
{

    //call from respository (uppermost) folder
    //dotnet ef migrations add remove_cloudflare_api -s Api -p databaseservices
    //dotnet ef database update -s Api -p databaseservices
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


            CreateMap<ApplicationUser, CreateUserDTO>().ReverseMap();

            CreateMap<Grouping, GroupingDTO>().ReverseMap();
            CreateMap<Grouping, GroupingRegistrationRequest>().ReverseMap();

            CreateMap<Device, DeviceDTO>().ReverseMap();









        }
    }
}
