using System.Net.Http.Headers;
using System.Net;
using Api.DependencyInjections.Authentication;
using Azure.Core;
using static Api.DependencyInjections.Authentication.AuthManager;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Models.Authentication;
using System.Net.Http.Headers;
using Microsoft.Net.Http.Headers;

namespace Api.CustomMiddlewares
{
    public class RefreshTokenDelegatingHandler : DelegatingHandler
    {
        private readonly IAuthManager _authManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;

        public RefreshTokenDelegatingHandler(IAuthManager authManager, IHttpContextAccessor httpContextAccessor, UserManager<ApplicationUser> userManager)
        {
            _authManager = authManager;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
        }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Call the inner handler to send the request
            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

            var cookieOptions = new CookieOptions
            {
                Expires = DateTimeOffset.Now.AddMinutes(2),
                IsEssential = true
            };

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {

           
                    RefreshTokenStatusResponse tokenResponse = await RefreshToken(request, response);
                    switch (tokenResponse.StatusCode)
                    {
                        case HttpStatusCode.OK:
                            response.Headers.Add("Set-Cookie", $"Authorization={tokenResponse.Token};{cookieOptions.ToString()}");
                   
                            // Update the original request with the new access token
                            if (request != null)
                            {
                                request.Headers.Authorization = new AuthenticationHeaderValue("accessToken", tokenResponse.Token);

                                // Call the inner handler to resend the original request with the new access token
                                response = await base.SendAsync(request, cancellationToken);
                            }
                            break;

                        case HttpStatusCode.BadRequest:
                            response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                            break;

                        default:
                            response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                            break;
                    }
          
            }

            return response;
        }


        private async Task<RefreshTokenStatusResponse> RefreshToken(HttpRequestMessage requestMessage, HttpResponseMessage responseMessage)
        {
            string refreshVal = string.Empty;
            RefreshTokenStatusResponse tokenResponse = new RefreshTokenStatusResponse() { StatusCode = HttpStatusCode.BadRequest };

            if (requestMessage == null || responseMessage == null)
                return tokenResponse;

            if (requestMessage.Headers.TryGetValues("Cookie", out IEnumerable<string> cookieValues))
            {
                var cookies = cookieValues.FirstOrDefault()?.Split(';').Select(cookie => cookie.Trim().Split('='));

                if (cookies != null)
                {
                    var refreshToken = cookies.FirstOrDefault(c => c[0] == "refreshToken")?.LastOrDefault();

                    if (!string.IsNullOrEmpty(refreshToken))
                    {
                        ClaimsPrincipal claim = _authManager.VerifyIfValidToken(refreshToken);
                        if (claim == null)
                            return tokenResponse;

                        var user = await _userManager.FindByNameAsync(claim.Identity.Name);

                        if (user == null || user.RefreshToken == null || user.RefreshTokenExpiryTime <= DateTime.Now)
                            return tokenResponse;

                        if (user.RefreshToken.ToLower() == refreshVal.ToLower())
                        {
                            // reuse
                        }

                        // create new access token and send back to user
                        string accessToken = await _authManager.CreateToken(user, false);

                        // add "Authorization" header to response message
                        responseMessage.Headers.Add("Authorization", accessToken);

                        tokenResponse.Token = accessToken;
                        tokenResponse.StatusCode = HttpStatusCode.OK;
                        return tokenResponse;
                    }
                }
            }

            return tokenResponse;
        }

    }

}
