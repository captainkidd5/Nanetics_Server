using Contracts.Authentication.Identity.Create;
using Models.Authentication;
using System.Security.Claims;
using static Api.DependencyInjections.Authentication.AuthManager;

namespace Api.DependencyInjections.Authentication
{
    public interface IAuthManager
    {
        Task<bool> ValidateUser(LoginUserDTO userDTO);
   
        Task<string> CreateToken(ApplicationUser user, bool isRefreshToken);
        //public string GenerateRefreshToken();
        ClaimsPrincipal? VerifyIfValidToken(string? token);

        Task<ApplicationUser> VerifyAccessTokenAndReturnuser(HttpRequest httpRequest, ClaimsPrincipal claimsPrinciple);
        Task<RefreshTokenStatusResponse> RefreshToken(HttpRequest request, HttpResponse response);
    }
}
