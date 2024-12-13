using System.Text;
using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Common.Settings;
using CleanArchitecture.Northwind.Domain.Entities.Identity;
using CleanArchitecture.Northwind.Infrastructure.Data;
using CleanArchitecture.Northwind.Infrastructure.Data.Interceptors;
using CleanArchitecture.Northwind.Infrastructure.Identity;
using CleanArchitecture.Northwind.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Microsoft.Extensions.DependencyInjection;
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration, bool useIdentityAuthorize = false)
    {
        #region 資料庫

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        Guard.Against.Null(connectionString, message: "Connection string 'DefaultConnection' not found.");

        services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
        services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());

            options.UseSqlServer(connectionString);
        });

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        services.AddScoped<ApplicationDbContextInitialiser>();

        // 分散式記憶體快取
        services.AddDistributedMemoryCache();

        #endregion

        #region 驗證

        if (useIdentityAuthorize)
            AddIdentityAuthorize(services, configuration);
        else
            AddJWTAuthorize(services, configuration);

        services.AddScoped<RoleManager<ApplicationRole>>();

        services.AddSingleton(System.TimeProvider.System);
        services.AddTransient<IIdentityService, IdentityService>();

        //services.AddAuthorization(options =>
        //    options.AddPolicy(Policies.CanPurge, policy => policy.RequireRole(Roles.Administrator)));

        #endregion

        #region 服務

        services.AddTransient<IFileService, FileService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddTransient<IDateTimeService, DateTimeService>();
        services.AddTransient<IMailService, MailService>();
        services.AddSingleton<ICurrentUserService, CurrentUserService>();

        #endregion

        #region 設置

        services.Configure<AppConfigurationSettings>(configuration.GetSection("AppConfigurationSettings"));
        services.Configure<JwtOptionSettings>(configuration.GetSection("JwtOptions"));
        services.Configure<MailSettings>(configuration.GetSection("MailSettings"));

        #endregion

        #region DataProtection

        // 配置 DataProtection 服務並持久化密鑰到本地文件系統
        services.AddDataProtection()
                .SetApplicationName(configuration["AppConfigurationSettings:SystemName"] ?? "")
                .PersistKeysToFileSystem(new DirectoryInfo(@"C:\keys"));

        // 讀取配置中的 Purpose 值
        var purpose = configuration["DataProtection:Purpose"] ?? "";

        // 註冊自定義的 DataProtectionService 並傳遞 purpose
        services.AddSingleton<IDataProtectionService>(provider =>
            new DataProtectionService(provider.GetRequiredService<IDataProtectionProvider>(), purpose));

        #endregion

        return services;
    }

    private static void AddIdentityAuthorize(IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
            {
                options.LoginPath = "/Account/Login";
                options.LogoutPath = "/Account/Logout";
                options.AccessDeniedPath = "/Account/AccessDenied";
                options.Cookie.HttpOnly = true;                             // 僅在 HTTPS 使用
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.SlidingExpiration = true;                           // 滑動過期時間
                options.ExpireTimeSpan = TimeSpan.FromHours(1);             // Cookie 過期時間
            });

        services.AddAuthorization();

        services
            .AddIdentity<ApplicationUser, ApplicationRole>(options =>
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
