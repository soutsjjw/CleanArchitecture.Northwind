using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Mvc.Infrastructure;
using CleanArchitecture.Northwind.Mvc.Services;
using CleanArchitecture.Northwind.Mvc.StartupExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using NToastNotify;

namespace Microsoft.Extensions.DependencyInjection;
public static class DependencyInjection
{
    public static IServiceCollection AddWebServices(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment env)
    {
        services.AddCustomizedSerilog(configuration);

        services.AddDatabaseDeveloperPageExceptionFilter();

        services.AddScoped<IUser, CurrentUser>();

        services.AddHttpContextAccessor();

        services.AddScoped<IUrlHelper>(s =>
        {
            var actionContext = s.GetRequiredService<IActionContextAccessor>().ActionContext;
            return new UrlHelper(actionContext);
        });

        services.AddExceptionHandler<CustomExceptionHandler>();

        var mvcBuilder = services.AddControllersWithViews();

        // 開發環境下，Razor 文件即時編譯
        if (env.IsDevelopment())
        {
            mvcBuilder.AddRazorRuntimeCompilation();
        }

        mvcBuilder.AddNToastNotifyToastr(new ToastrOptions()
        {
            ProgressBar = false,
            PositionClass = ToastPositions.BottomRight
        });

        services.AddEndpointsApiExplorer();

        services.AddCustomizedMiddleware();

        return services;
    }
}
