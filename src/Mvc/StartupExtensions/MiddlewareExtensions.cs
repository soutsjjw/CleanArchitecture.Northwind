
using CleanArchitecture.Northwind.Infrastructure.Middleware;

namespace CleanArchitecture.Northwind.Mvc.StartupExtensions;

public static class MiddlewareExtensions
{
    public static IServiceCollection AddCustomizedMiddleware(this IServiceCollection services)
    {
        services.AddScoped<ExceptionHandlerMiddleware>();

        return services;
    }

    public static IApplicationBuilder UseCustomizedMiddleware(this IApplicationBuilder app)
    {
        app.UseMiddleware<ExceptionHandlerMiddleware>();

        return app;
    }
}
