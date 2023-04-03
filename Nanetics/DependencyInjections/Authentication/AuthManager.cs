using Contracts.Authentication.Identity.Create;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using Models.Authentication;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace SilverMenu.DependencyInjections.Authentication
{
    public class AuthManager : IAuthManager
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly RoleManager<ApplicationRole> _rolesManager;

        public AuthManager(UserManager<ApplicationUser> userManager,
            IConfiguration configuration, RoleManager<ApplicationRole> rolesManager)
        {
            _userManager = userManager;
            _configuration = configuration;
            _rolesManager = rolesManager;
        }
        public async Task<string> CreateToken(ApplicationUser user, bool isRefreshToken)
        {
            SigningCredentials signingCredentials = GetSigningCredentials();

            //minutes
            int expires = 1;

            if (isRefreshToken)
                expires = 120;

            SecurityTokenDescriptor tokenDescription = await GenerateToken(user, signingCredentials, expires);

            

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken token = tokenHandler.CreateToken(tokenDescription);
            return tokenHandler.WriteToken(token);
        }

        private async Task<SecurityTokenDescriptor> GenerateToken(ApplicationUser user, SigningCredentials signingCredentials, int expiryTime)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            Guid guid = Guid.NewGuid();
            //int minutes = int.Parse(jwtSettings.GetSection("Lifetime").Value);
            int minutes = expiryTime;

            SecurityTokenDescriptor token = new SecurityTokenDescriptor()
            {
                Issuer = jwtSettings.GetSection("Issuer").Value,
                Subject = new ClaimsIdentity(await GetValidClaims(user)),
                Audience = _configuration.GetSection("HostPath").Value,


                Expires = DateTime.UtcNow.Add(TimeSpan.FromMinutes(minutes)),
                SigningCredentials = signingCredentials
            };


            return token;
        }
        private async Task<List<Claim>> GetValidClaims(ApplicationUser user)
        {
            IdentityOptions _options = new IdentityOptions();
            var claims = new List<Claim>
    {
        new Claim(JwtRegisteredClaimNames.Email, user.Email),
        new Claim(_options.ClaimsIdentity.UserIdClaimType, user.Id.ToString()),
        new Claim(_options.ClaimsIdentity.UserNameClaimType, user.UserName),
    };

            var userClaims = await _userManager.GetClaimsAsync(user);
            var userRoles = await _userManager.GetRolesAsync(user);
            claims.AddRange(userClaims);
            foreach (var userRole in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, userRole));
                var role = await _rolesManager.FindByNameAsync(userRole);
                if (role != null)
                {
                    var roleClaims = await _rolesManager.GetClaimsAsync(role);
                    foreach (Claim roleClaim in roleClaims)
                    {
                        claims.Add(roleClaim);
                    }
                }
            }
            return claims;
        }

        private SigningCredentials GetSigningCredentials()
        {
            var encodedKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("Jwt").GetSection("Key").Value));
            return new SigningCredentials(encodedKey, SecurityAlgorithms.HmacSha256);
        }
        public async Task<bool> ValidateUser(LoginUserDTO userDTO)
        {
            ApplicationUser user = await _userManager.FindByNameAsync(userDTO.Email);
            return (user != null && await _userManager.CheckPasswordAsync(user, userDTO.Password));
        }

  

        //public string GenerateRefreshToken()
        //{
        //    byte[] randomNumber = new byte[64];
        //    using RandomNumberGenerator rng = RandomNumberGenerator.Create();
        //    rng.GetBytes(randomNumber);
        //    return Convert.ToBase64String(randomNumber);
        //}

        public ClaimsPrincipal? VerifyIfValidToken(string? token)
        {
            try
            {
                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = false,
                    ValidateIssuer = false,
                    ValidateIssuerSigningKey = false,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("Jwt").GetSection("Key").Value)),
                    ValidateLifetime = false
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
                if (securityToken is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                    throw new SecurityTokenException("Invalid token");

                return principal;
            }
            catch(Exception e)
            {
                return null;
            }
            

        }

        public async Task<ApplicationUser> VerifyRefreshTokenAndReturnUser(HttpRequest httpRequest)
        {

            StringValues strValues = new StringValues();

            if (!httpRequest.Headers.TryGetValue("refreshToken", out strValues))
                return null;

            string newVal = strValues.First().Replace("{", "").Replace("}", "").Replace("Bearer ", "");
            ClaimsPrincipal claim = VerifyIfValidToken(newVal);
            if (claim == null)
                return null;


            var user = await _userManager.FindByNameAsync(claim.Identity.Name);

            if (user == null || user.RefreshToken == null
                || user.RefreshTokenExpiryTime <= DateTime.Now)
            {
                return null;
            }

            return user;
        }
    }
}
