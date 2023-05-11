using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

public class LogUnauthorizedRequestFilter : IAsyncExceptionFilter
{
    private readonly ILogger<LogUnauthorizedRequestFilter> _logger;

    public LogUnauthorizedRequestFilter(ILogger<LogUnauthorizedRequestFilter> logger)
    {
        _logger = logger;
    }

    public async Task OnExceptionAsync(ExceptionContext context)
    {
        if (context.Exception is UnauthorizedAccessException)
        {
            // Log all headers
            var headers = new Dictionary<string, string>();
            foreach (var header in context.HttpContext.Request.Headers)
            {
                headers.Add(header.Key, header.Value.ToString());
            }
            var requestBody = string.Empty;
            context.HttpContext.Request.EnableBuffering();
            using (var reader = new StreamReader(context.HttpContext.Request.Body, Encoding.UTF8, true, 1024, true))
            {
                requestBody = await reader.ReadToEndAsync();
                context.HttpContext.Request.Body.Position = 0;
            }
            _logger.LogInformation("Controller: {Controller_Action}, HTTP Method: {Http_Method}, RequestHeaders: {@Request_Headers}, RequestBody: {Request_Body}",
          context.HttpContext.GetEndpoint(),
          context.HttpContext.Request.Method,
          context.HttpContext.Request.Headers,
          requestBody);
            // Return a 401 response
            context.HttpContext.Response.StatusCode = 401;
            await context.HttpContext.Response.CompleteAsync();
        }
    }
}
