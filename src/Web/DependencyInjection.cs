using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Web.Filters;
using CleanArchitecture.Northwind.Web.Services;
using CleanArchitecture.Northwind.Web.StartupExtensions;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static void AddWebServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddCustomizedSerilog(builder.Configuration);

        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        builder.Services.AddScoped<IUser, CurrentUser>();

        builder.Services.AddHttpContextAccessor();

        builder.Services.AddExceptionHandler<CustomExceptionHandler>();
        builder.Services.AddExceptionHandler<ProblemDetailsExceptionHandler>();

        builder.Services.AddScoped<AntiforgeryRedirectFilter>();
        var mvcBuilder = builder.Services.AddControllersWithViews(options =>
        {
            // 注意：ActionFilter 要先於 ExceptionFilter 執行
            options.Filters.Add<StoreActionArgumentsFilter>();
            options.Filters.Add<ValidationExceptionFilter>();
            options.Filters.AddService<AntiforgeryRedirectFilter>();
        });

        // 開發環境下，Razor 文件即時編譯
        if (builder.Environment.IsDevelopment())
        {
            mvcBuilder.AddRazorRuntimeCompilation();
        }

        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddCustomizedMiddleware();



        builder.Services.AddCors();
    }
}
