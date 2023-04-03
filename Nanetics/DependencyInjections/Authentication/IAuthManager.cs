using Contracts.Authentication.Identity.Create;
using Models.Authentication;
using System.Security.Claims;

namespace SilverMenu.DependencyInjections.Authentication
{
    public interface IAuthManager
    {
        Task<bool> ValidateUser(LoginUserDTO userDTO);
   
        Task<string> CreateToken(ApplicationUser user, bool isRefreshToken);
        //public string GenerateRefreshToken();
        ClaimsPrincipal? VerifyIfValidToken(string? token);

        Task<ApplicationUser> VerifyRefreshTokenAndReturnUser(HttpRequest httpRequest);
    }
}
