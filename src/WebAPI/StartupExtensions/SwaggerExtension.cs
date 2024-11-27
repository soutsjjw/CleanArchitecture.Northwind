using NSwag;
using NSwag.Generation.AspNetCore;
using NSwag.Generation.Processors.Security;

namespace CleanArchitecture.Northwind.WebAPI.StartupExtensions;

public static class SwaggerExtension
{
    public static IServiceCollection AddCustomizedSwagger(this IServiceCollection services, IWebHostEnvironment env)
    {
        if (!env.IsDevelopment())
            return services;

        services.AddOpenApiDocument(configure =>
        {
            SetOpenApiDocument(configure, 1);
        });

        services.AddOpenApiDocument(configure =>
        {
            SetOpenApiDocument(configure, 2);
        });

        return services;
    }

    private static void SetOpenApiDocument(AspNetCoreOpenApiDocumentGeneratorSettings configure, int version)
    {
        configure.DocumentName = $"v{version}";
        configure.ApiGroupNames = new[] { $"v{version}" };
        configure.PostProcess = document =>
        {
            document.Info.Version = $"v{version}";
            document.Info.Title = $"CleanArchitecture.Northwind WebAPI - v{version}";
            document.Info.Description = $"A simple ASP.NET Core Web API - Version {version}";
        };

        // Add JWT
        configure.AddSecurity("JWT", Enumerable.Empty<string>(), new OpenApiSecurityScheme
        {
            Type = OpenApiSecuritySchemeType.ApiKey,
            Name = "Authorization",
            In = OpenApiSecurityApiKeyLocation.Header,
            Description = "Type into the textbox: Bearer {your JWT token}."
        });

        configure.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor("JWT"));
    }

    public static IApplicationBuilder UseCustomizedSwagger(this IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseOpenApi();
            app.UseSwaggerUi(settings =>
            {
                settings.Path = "/api";

                // 設置默認選定版本為v2，所以v2放在第一個
                settings.SwaggerRoutes.Add(new NSwag.AspNetCore.SwaggerUiRoute("v2", "/swagger/v2/swagger.json"));
                settings.SwaggerRoutes.Add(new NSwag.AspNetCore.SwaggerUiRoute("v1", "/swagger/v1/swagger.json"));
            });
        }

        return app;
    }
}
