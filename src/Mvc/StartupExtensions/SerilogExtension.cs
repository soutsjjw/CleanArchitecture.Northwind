using Serilog;
using Serilog.Sinks.MSSqlServer;

namespace CleanArchitecture.Northwind.Mvc.StartupExtensions;

public static class SerilogExtension
{
    public static IServiceCollection AddCustomizedSerilog(this IServiceCollection services, IConfiguration configuration)
    {
        var envConnectionStringKey = configuration.GetConnectionString("DefaultConnection");
        var connectionString = Environment.GetEnvironmentVariable(envConnectionStringKey ?? "");

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .WriteTo.MSSqlServer(
                connectionString: connectionString,
                sinkOptions: new MSSqlServerSinkOptions
                {
                    TableName = "Logs",
                    AutoCreateSqlTable = true
                }
            )
            .CreateLogger();

        services.AddSerilog();

        return services;
    }
}
