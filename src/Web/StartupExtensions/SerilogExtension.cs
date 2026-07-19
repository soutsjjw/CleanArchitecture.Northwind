using Serilog;
using Serilog.Sinks.MSSqlServer;

namespace CleanArchitecture.Northwind.Web.StartupExtensions;

public static class SerilogExtension
{
    public static IServiceCollection AddCustomizedSerilog(this IServiceCollection services, IConfiguration configuration)
    {
        var envConnectionStringKey = configuration.GetConnectionString("DefaultConnection");
        var connectionString = Environment.GetEnvironmentVariable(envConnectionStringKey ?? "");

        var loggerConfig = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration);

        // 只有在資料庫連線可用時才啟用 SQL Server sink
        if (!string.IsNullOrEmpty(connectionString))
        {
            try
            {
                loggerConfig.WriteTo.MSSqlServer(
                    connectionString: connectionString,
                    sinkOptions: new MSSqlServerSinkOptions
                    {
                        TableName = "Logs",
                        AutoCreateSqlTable = true
                    }
                );
            }
            catch
            {
                // 如果資料庫連線失敗,僅使用檔案和 Console 記錄
            }
        }

        Log.Logger = loggerConfig.CreateLogger();

        services.AddSerilog();

        return services;
    }
}
