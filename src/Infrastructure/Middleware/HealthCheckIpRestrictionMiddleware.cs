using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace CleanArchitecture.Northwind.Infrastructure.Middleware;

public class HealthCheckIpRestrictionMiddleware : IMiddleware
{
    private readonly string _Uri;
    private readonly string _UIPath;
    private readonly string _ApiPath;
    private readonly List<string> _AllowedIps;

    public HealthCheckIpRestrictionMiddleware(IConfiguration configuration)
    {
        _AllowedIps = configuration.GetSection("HealthChecksUI:AllowIPs").Get<string[]>()?.ToList() ?? new List<string>();
        _Uri = configuration["HealthChecksUI:HealthChecks:0:Uri"] ?? "";
        _UIPath = configuration["HealthChecksUI:UIPath"] ?? "";
        _ApiPath = configuration["HealthChecksUI:ApiPath"] ?? "";
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (_AllowedIps == null || _AllowedIps.Count == 0)
            await next(context);

        var path = context.Request.Path.ToString().ToLower();
        if (path.StartsWith(_Uri) || path.StartsWith(_UIPath) || path.StartsWith(_ApiPath))
        {
            var remoteIp = context.Connection.RemoteIpAddress?.ToString();
            if (!_AllowedIps.Contains(remoteIp))
            {
                //TODO: 調整為制式的回應內容
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsync("Forbidden");
                return;
            }
        }

        await next(context);
    }
}
