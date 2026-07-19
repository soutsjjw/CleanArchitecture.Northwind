using System.Data;
using System.Text;
using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Common.Interfaces.Database;
using CleanArchitecture.Northwind.Application.Common.Interfaces.Identity;
using CleanArchitecture.Northwind.Application.Common.Interfaces.Repository;
using CleanArchitecture.Northwind.Application.Common.Models;
using CleanArchitecture.Northwind.Application.Common.Settings;
using CleanArchitecture.Northwind.Domain.Entities;
using CleanArchitecture.Northwind.Domain.Entities.Identity;
using CleanArchitecture.Northwind.Infrastructure.Authorization;
using CleanArchitecture.Northwind.Infrastructure.Configurations;
using CleanArchitecture.Northwind.Infrastructure.Data;
using CleanArchitecture.Northwind.Infrastructure.Data.Interceptors;
using CleanArchitecture.Northwind.Infrastructure.Identity;
using CleanArchitecture.Northwind.Infrastructure.Repository;
using CleanArchitecture.Northwind.Infrastructure.Services;
using CleanArchitecture.Northwind.Infrastructure.Services.Database;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static void AddInfrastructureServices(this IHostApplicationBuilder builder, bool useIdentityAuthorize = false)
    {
        #region 資料庫

        var envConnectionStringKey = builder.Configuration.GetConnectionString("DefaultConnection");
        var connectionString = Environment.GetEnvironmentVariable(envConnectionStringKey ?? "");

        Guard.Against.Null(connectionString, message: "Connection string 'DefaultConnection' not found.");

        builder.Services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
        builder.Services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();

        builder.Services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());

            options.UseSqlServer(connectionString);
            options.ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
        });

        builder.Services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        builder.Services.AddScoped<ApplicationDbContextInitialiser>();

        // 分散式記憶體快取
        builder.Services.AddDistributedMemoryCache();

        #endregion

        #region 驗證

        if (useIdentityAuthorize)
            AddIdentityAuthorize(builder.Services, builder.Configuration);
        else
            AddJWTAuthorize(builder.Services, builder.Configuration);

        builder.Services.AddScoped<RoleManager<ApplicationRole>>();

        builder.Services.AddSingleton(System.TimeProvider.System);
        builder.Services.AddTransient<IIdentityService, IdentityService>();

        builder.Services.AddAuthorization();

        builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();

        builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
        // 既有（精確 + 可帶資源）Handler
        builder.Services.AddScoped<IAuthorizationHandler, PermissionResourceAuthorizationHandler>();

        #endregion

        builder.Services.AddScoped<IDbConnection>(sp => new SqlConnection(connectionString));

        // 註冊 DapperRepository
        builder.Services.AddScoped(typeof(IRepository<Order>), typeof(OrdersRepository));
        builder.Services.AddScoped(typeof(IUserProfileRepository), typeof(UserProfileRepository));

        builder.Services.AddTransient<IOrdersService, OrdersService>();

        #region 服務

        builder.Services.AddTransient<IFileService, FileService>();
        builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
        builder.Services.AddTransient<IDateTimeService, DateTimeService>();
        builder.Services.AddTransient<IMailService, MailService>();
        builder.Services.AddSingleton<ICurrentUserService, CurrentUserService>();
        builder.Services.AddSingleton<ICloudflareService, CloudflareService>();
        builder.Services.AddScoped<ICommonService, CommonService>();
        builder.Services.AddSingleton<IExcelExporter, ExcelExporter>();
        builder.Services.AddSingleton<IAppLogFileService, SerilogAppLogFileService>();

        #endregion

        builder.Services.AddMemoryCache();
        builder.Services.Configure<PermissionResolverOptions>(o =>
        {
            o.CacheSeconds = 60;                        // 0 關閉快取
            o.ClaimType = PermissionConstants.ClaimType;
        });

        builder.Services.AddScoped<IPermissionResolver, PermissionDbResolver>();

        #region 設置

        builder.Services.Configure<AppConfigurationSettings>(builder.Configuration.GetSection("AppConfigurationSettings"))
            .AddSingleton(s => s.GetRequiredService<IOptions<AppConfigurationSettings>>().Value)
            .AddSingleton<IAppConfigurationSettings>(s => s.GetRequiredService<IOptions<AppConfigurationSettings>>().Value);

        builder.Services.Configure<IdentitySettings>(builder.Configuration.GetSection("IdentitySettings"))
            .AddSingleton(s => s.GetRequiredService<IOptions<IdentitySettings>>().Value)
            .AddSingleton<IIdentitySettings>(s => s.GetRequiredService<IOptions<IdentitySettings>>().Value);

        builder.Services.Configure<JwtOptionSettings>(builder.Configuration.GetSection("JwtOptions"));
        builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));
        builder.Services.Configure<CloudflareOptions>(builder.Configuration.GetSection("Cloudflare"));
        builder.Services.Configure<DataProtectionSettings>(builder.Configuration.GetSection("DataProtection"));

        #endregion

        #region DataProtection

        // 配置 DataProtection 服務並持久化密鑰到本地文件系統
        builder.Services.AddDataProtection()
                .SetApplicationName(builder.Configuration["AppConfigurationSettings:SystemName"] ?? "")
                .PersistKeysToFileSystem(new DirectoryInfo(@"C:\keys"));

        // 讀取配置中的 Purpose 值
        var purpose = builder.Configuration["DataProtection:Purpose"] ?? "";

        // 註冊自定義的 DataProtectionService 並傳遞 purpose
        builder.Services.AddSingleton<IDataProtectionService>(provider =>
            new DataProtectionService(provider.GetRequiredService<IDataProtectionProvider>(), purpose));

        #endregion
    }

    private static void AddIdentityAuthorize(IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme);

        services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/Account/Login";
            options.LogoutPath = "/Account/Logout";
            options.AccessDeniedPath = "/Error/AccessDenied";
            options.Cookie.HttpOnly = true;                             // 僅在 HTTPS 使用
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.SlidingExpiration = true;                           // 滑動過期時間
            options.ExpireTimeSpan = TimeSpan.FromHours(8);             // Cookie 過期時間
        });

        services.AddAuthorizationBuilder();

        services
            .AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                var identitySettings = configuration.GetRequiredSection("IdentitySettings").Get<IdentitySettings>();
                identitySettings = identitySettings ?? new IdentitySettings();

                // SignIn 設定
                options.SignIn.RequireConfirmedEmail = true;
                options.SignIn.RequireConfirmedPhoneNumber = false;

                // 密碼規則設定
                options.Password.RequireDigit = identitySettings.RequireDigit;
                options.Password.RequiredLength = identitySettings.RequiredLength;
                options.Password.RequireNonAlphanumeric = identitySettings.RequireNonAlphanumeric;
                options.Password.RequireUppercase = identitySettings.RequireUpperCase;
                options.Password.RequireLowercase = identitySettings.RequireLowerCase;
                options.Password.RequiredUniqueChars = identitySettings.RequiredUniqueChars;

                // 鎖定設定
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(identitySettings.DefaultLockoutTimeSpan);
                options.Lockout.MaxFailedAccessAttempts = identitySettings.MaxFailedAccessAttempts;
                options.Lockout.AllowedForNewUsers = identitySettings.AllowedForNewUsers;

                // 使用 Email 傳遞密碼重置令牌
                options.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultEmailProvider;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddApiEndpoints();
    }

    private static void AddJWTAuthorize(IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(options =>
        {
            // 指定默認的身份驗證方案，使用 JWT Bearer 的身份驗證方案來處理身份驗證請求
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            // 指定默認的挑戰方案，使用 JWT Bearer 的身份驗證方案來處理身份驗證挑戰
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
            .AddJwtBearer(options =>
            {
                var key = configuration["JwtOptions:Key"] ?? "";
                var issuer = configuration["JwtOptions:Issuer"] ?? "";
                var audience = configuration["JwtOptions:Audience"] ?? "";

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                    // 指定從 JWT 聲明中提取哪個字段作為用戶名
                    NameClaimType = "name",
                    ValidAlgorithms = new string[] { "HS512" },
                    // 允許服務器和客戶端之間的時間不同步，避免因小的時間偏移而導致 JWT 驗證失敗
                    ClockSkew = TimeSpan.Zero,
                };
            });

        services.AddAuthorization();

        /*
         * servicesAddIdentity<ApplicationUser, ApplicationRole>()
         * 如果使用此種方式，驗證方案將會有下列方案
         * - Bearer
         * - Identity.Application
         * - Identity.External
         * - Identity.TwoFactorRememberMe
         * - Identity.TwoFactorUserId
        */
        services
            .AddIdentityCore<ApplicationUser>(options =>
            {
                // SignIn 設定
                options.SignIn.RequireConfirmedEmail = true;
                options.SignIn.RequireConfirmedPhoneNumber = false;

                // 密碼規則設定
                options.Password.RequireDigit = true;                               // 是否需要數字
                options.Password.RequiredLength = 12;                               // 最小長度
                options.Password.RequireNonAlphanumeric = true;                     // 是否需要非字母數字字符
                options.Password.RequireUppercase = true;                           // 是否需要大寫字母
                options.Password.RequireLowercase = true;                           // 是否需要小寫字母
                options.Password.RequiredUniqueChars = 4;                           // 需要的唯一字符數量

                // 鎖定設定
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);  // 鎖定時間
                options.Lockout.MaxFailedAccessAttempts = 5;                        // 登入失敗次數
                options.Lockout.AllowedForNewUsers = true;                          // 新使用者是否可以被鎖定

                // 使用 Email 傳遞密碼重置令牌
                options.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultEmailProvider;
            })
            .AddRoles<ApplicationRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddApiEndpoints();
    }
}
