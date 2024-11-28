
using CleanArchitecture.Northwind.Infrastructure.Middleware;

namespace CleanArchitecture.Northwind.WebAPI.StartupExtensions;

public static class MiddlewareExtensions
{
    public static IServiceCollection AddCustomizedMiddleware(this IServiceCollection services)
    {
        services.AddScoped<ExceptionHandlerMiddleware>();
        services.AddScoped<HealthCheckIpRestrictionMiddleware>();

        return services;
    }

    public static IApplicationBuilder UseCustomizedMiddleware(this IApplicationBuilder app)
    {
        app.UseMiddleware<ExceptionHandlerMiddleware>();
        app.UseMiddleware<HealthCheckIpRestrictionMiddleware>();

        return app;
    }
}
