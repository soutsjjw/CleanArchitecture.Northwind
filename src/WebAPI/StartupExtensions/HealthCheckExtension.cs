using CleanArchitecture.Northwind.Infrastructure.Data;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace CleanArchitecture.Northwind.WebAPI.StartupExtensions;

public static class HealthCheckExtension
{
    public static IServiceCollection AddCustomizedHealthCheck(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment env)
    {
        if (env.IsProduction() || env.IsStaging())
        {
            services.AddHealthChecks()
                .AddSqlServer(configuration.GetConnectionString("DefaultConnection") ?? "")
                .AddDbContextCheck<ApplicationDbContext>();

            services.AddHealthChecksUI(opt =>
            {
                opt.SetEvaluationTimeInSeconds(15);
            }).AddInMemoryStorage();
        }

        return services;
    }

    public static void UseCustomizedHealthCheck(IEndpointRouteBuilder endpoints, IConfiguration configuration, IWebHostEnvironment env)
    {
        var uri = configuration["HealthChecksUI:HealthChecks:0:Uri"] ?? "";
        var uiPath = configuration["HealthChecksUI:UIPath"] ?? "";
        var apiPath = configuration["HealthChecksUI:ApiPath"] ?? "";

        if (env.IsProduction() || env.IsStaging())
        {
            endpoints.MapHealthChecks(uri, new HealthCheckOptions
            {
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
            });

            endpoints.MapHealthChecksUI(setup =>
            {
                setup.UIPath = uiPath;
                setup.ApiPath = apiPath;
            });
        }
    }
}
