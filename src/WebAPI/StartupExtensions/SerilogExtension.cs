using Serilog;

namespace CleanArchitecture.Northwind.WebAPI.StartupExtensions;

public static class SerilogExtension
{
    public static IServiceCollection AddCustomizedSerilog(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSerilog(options =>
        {
            /// TODO: 連線字串不在 appsettings.json 中設置
            options.ReadFrom.Configuration(configuration);
        });

        return services;
    }
}
