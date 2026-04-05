using System.Reflection;
using CleanArchitecture.Northwind.Application.Common.Behaviours;
using CleanArchitecture.Northwind.Application.Common.Mappings;
using Mapster;
using MapsterMapper;

namespace Microsoft.Extensions.DependencyInjection;
public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddSingleton(CreateTypeAdapterConfig());
        services.AddScoped<IMapper, ServiceMapper>();

        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehaviour<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehaviour<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(PerformanceBehaviour<,>));
        });

        return services;
    }

    private static TypeAdapterConfig CreateTypeAdapterConfig()
    {
        TypeAdapterConfig config = new();
        MapsterConfiguration.RegisterMappings(config);
        config.Compile();
        return config;
    }
}
