namespace CleanArchitecture.Northwind.WebAPI.Middleware;

public class HealthCheckIpRestrictionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string _Uri;
    private readonly string _UIPath;
    private readonly string _ApiPath;
    private readonly List<string> _AllowedIps;

    public HealthCheckIpRestrictionMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _AllowedIps = configuration.GetSection("HealthChecksUI:AllowIPs").Get<string[]>()?.ToList() ?? new List<string>();
        _Uri = configuration["HealthChecksUI:HealthChecks:0:Uri"] ?? "";
        _UIPath = configuration["HealthChecksUI:UIPath"] ?? "";
        _ApiPath = configuration["HealthChecksUI:ApiPath"] ?? "";
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (_AllowedIps == null || _AllowedIps.Count == 0)
            await _next(context);

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

        await _next(context);
    }
}
