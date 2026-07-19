using System.Reflection;
using CleanArchitecture.Northwind.Application.Common.Behaviours;
using CleanArchitecture.Northwind.Application.Common.Mappings;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddSingleton(CreateTypeAdapterConfig());
        builder.Services.AddScoped<IMapper, ServiceMapper>();

        builder.Services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            cfg.AddOpenRequestPreProcessor(typeof(LoggingBehaviour<>));
            cfg.AddOpenBehavior(typeof(UnhandledExceptionBehaviour<,>));
            cfg.AddOpenBehavior(typeof(AuthorizationBehaviour<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehaviour<,>));
            cfg.AddOpenBehavior(typeof(PerformanceBehaviour<,>));
        });
    }

    private static TypeAdapterConfig CreateTypeAdapterConfig()
    {
        TypeAdapterConfig config = new();
        MapsterConfiguration.RegisterMappings(config);
        config.Compile();
        return config;
    }
}
