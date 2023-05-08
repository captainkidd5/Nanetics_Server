using Azure.Core;
using Contracts.Authentication.Identity.Create;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using Models.Authentication;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Api.DependencyInjections.Authentication
{
    public class AuthManager : IAuthManager
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly RoleManager<ApplicationRole> _rolesManager;
        private readonly IWebHostEnvironment _env;

        public AuthManager(UserManager<ApplicationUser> userManager,
            IConfiguration configuration, RoleManager<ApplicationRole> rolesManager, IWebHostEnvironment env)
        {
            _userManager = userManager;
            _configuration = configuration;
            _rolesManager = rolesManager;
            _env = env;
        }
        public async Task<string> CreateToken(ApplicationUser user, bool isRefreshToken)
        {
            SigningCredentials signingCredentials = GetSigningCredentials();

            //minutes
            //  int expires = 1;
            DateTime expiryTime = DateTime.Now.AddDays(7);
            if (!isRefreshToken)
                expiryTime = DateTime.Now.AddMinutes(2);

            SecurityTokenDescriptor tokenDescription = await GenerateToken(user, signingCredentials, DateTime.Now.AddDays(7));

            

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken token = tokenHandler.CreateToken(tokenDescription);
            return tokenHandler.WriteToken(token);
        }

        private async Task<SecurityTokenDescriptor> GenerateToken(ApplicationUser user, SigningCredentials signingCredentials, DateTime expireTime)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            Guid guid = Guid.NewGuid();
            //int minutes = int.Parse(jwtSettings.GetSection("Lifetime").Value);

            SecurityTokenDescriptor token = new SecurityTokenDescriptor()
            {
                Issuer = jwtSettings.GetSection("Issuer").Value,
                Subject = new ClaimsIdentity(await GetValidClaims(user)),
                Audience = _configuration.GetSection("HostPath").Value,


                Expires = expireTime,
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

        public class RefreshTokenStatusResponse
        {
            public HttpStatusCode StatusCode { get; set; }
            public string Token { get; set; }
        }
        public async Task<RefreshTokenStatusResponse> RefreshToken(HttpRequest request, HttpResponse response)
        {
            //TODO: Handle edge cases found here:
            //https://github.com/gitdagray/nodejs_jwt_auth/blob/main/controllers/refreshTokenController.js


            string refreshVal = string.Empty;
            RefreshTokenStatusResponse tokenResponse = new RefreshTokenStatusResponse() { StatusCode = HttpStatusCode.BadRequest };
            if (!request.Cookies.TryGetValue("refreshToken", out refreshVal))
                return tokenResponse;


            ClaimsPrincipal claim = VerifyIfValidToken(refreshVal);
            if (claim == null)
                return tokenResponse;


            var user = await _userManager.FindByNameAsync(claim.Identity.Name);

            if (user == null || user.RefreshToken == null
                || user.RefreshTokenExpiryTime <= DateTime.Now)
            {
                return tokenResponse;
            }
            if (user.RefreshToken.ToLower() == refreshVal.ToLower())
            {
                //reuse
            }



            //create new access token and send back to user
            string accessToken = await CreateToken(user, false);


            //Response.Cookies.Append("Authorization", accessToken,
            //    new CookieOptions() { Expires = DateTimeOffset.Now.AddMinutes(2), IsEssential = true });

            tokenResponse.Token = accessToken;
            tokenResponse.StatusCode = HttpStatusCode.OK;
            return tokenResponse;

        }


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
        /// <summary>
        /// Returns the application user if both the refresh token and the claimsprinciple verify the user.
        /// Development will only check the claims principle
        /// </summary>
        /// <param name="httpRequest"></param>
        /// <param name="claimsPrincipal"></param>
        /// <returns></returns>
        public async Task<ApplicationUser> VerifyAccessTokenAndReturnuser(HttpRequest httpRequest, ClaimsPrincipal claimsPrincipal)
        {
            //Dev environment is incapable of using refresh tokens, just return the user
            if (_env.IsDevelopment())
                return await _userManager.GetUserAsync(claimsPrincipal);
            StringValues strValues = new StringValues();

            if (!httpRequest.Headers.TryGetValue("accessToken", out strValues))
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
