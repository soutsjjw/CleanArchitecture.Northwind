using System.Text;
using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Domain.Constants;
using CleanArchitecture.Northwind.Infrastructure.Data;
using CleanArchitecture.Northwind.Infrastructure.Data.Interceptors;
using CleanArchitecture.Northwind.Infrastructure.Identity;
using CleanArchitecture.Northwind.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Microsoft.Extensions.DependencyInjection;
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
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

        services.AddTransient<IDateTimeService, DateTimeService>();

        //services
        //.AddAuthentication()
        //.AddBearerToken(IdentityConstants.BearerScheme)

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

        services.AddAuthorizationBuilder();

        services
            .AddIdentityCore<ApplicationUser>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddApiEndpoints();

        services.AddScoped<RoleManager<IdentityRole>>();

        services.AddSingleton(System.TimeProvider.System);
        services.AddTransient<IIdentityService, IdentityService>();

        services.AddAuthorization(options =>
            options.AddPolicy(Policies.CanPurge, policy => policy.RequireRole(Roles.Administrator)));


        services.AddScoped<IJwtTokenService, JwtTokenService>();

        return services;
    }
}
