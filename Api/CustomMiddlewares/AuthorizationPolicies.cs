using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore.Internal;
using System.Security.Claims;
using System.Text;

namespace Api.CustomMiddlewares
{
    public class AuthorizationPolicies : IAuthorizationHandler
    {
        private readonly ILogger<AuthorizationPolicies> _logger;

        public AuthorizationPolicies(ILogger<AuthorizationPolicies> logger)
        {
            _logger = logger;
        }
        public async Task HandleAsync(AuthorizationHandlerContext context)
        {
            if (context.Resource is HttpContext httpContext)
            {
                var headers = new Dictionary<string, string>();
                foreach (var header in httpContext.Request.Headers)
                {
                    headers.Add(header.Key, header.Value.ToString());
                }
                var requestBody = string.Empty;
                httpContext.Request.EnableBuffering();
                using (var reader = new StreamReader(httpContext.Request.Body, Encoding.UTF8, true, 1024, true))
                {
                    requestBody = await reader.ReadToEndAsync();
                    httpContext.Request.Body.Position = 0;
                }
                _logger.LogInformation("Controller: {Controller_Action}, HTTP Method: {Http_Method}, RequestHeaders: {@Request_Headers}, RequestBody: {Request_Body}",
              httpContext.GetEndpoint(),
              httpContext.Request.Method,
              httpContext.Request.Headers,
              requestBody);
            }
            await Console.Out.WriteLineAsync("test");
            //foreach (IAuthorizationRequirement requirement in context.Requirements)
            //{
               
            //    await requirement.HandleAsync(context);
            //}
        }
    }
}
