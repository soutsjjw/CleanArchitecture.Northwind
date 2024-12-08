using Azure.Identity;
using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.WebAPI.Services;
using CleanArchitecture.Northwind.WebAPI.StartupExtensions;
using Microsoft.AspNetCore.Mvc;

namespace Microsoft.Extensions.DependencyInjection;
public static class DependencyInjection
{
    public static IServiceCollection AddWebServices(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment env)
    {
        services.AddCustomizedSerilog(configuration);

        services.AddDatabaseDeveloperPageExceptionFilter();

        services.AddScoped<IUser, CurrentUser>();

        services.AddHttpContextAccessor();

        services.AddCustomizedHealthCheck(configuration, env);

        services.AddExceptionHandler<CustomExceptionHandler>();

        services.AddControllers();

        // Customise default API behaviour
        services.Configure<ApiBehaviorOptions>(options =>
            options.SuppressModelStateInvalidFilter = true);

        services.AddEndpointsApiExplorer();

        services.AddCustomizedApiVersion();

        services.AddCustomizedSwagger(env);

        services.AddCustomizedMiddleware();

        return services;
    }

    public static IServiceCollection AddKeyVaultIfConfigured(this IServiceCollection services, ConfigurationManager configuration)
    {
        var keyVaultUri = configuration["AZURE_KEY_VAULT_ENDPOINT"];
        if (!string.IsNullOrWhiteSpace(keyVaultUri))
        {
            configuration.AddAzureKeyVault(
                new Uri(keyVaultUri),
                new DefaultAzureCredential());
        }

        return services;
    }
}
