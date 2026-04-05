using CleanArchitecture.Northwind.Infrastructure.Configurations;
using CleanArchitecture.Northwind.Infrastructure.Data;
using CleanArchitecture.Northwind.Mvc.StartupExtensions;
using Microsoft.AspNetCore.HttpOverrides;
using Mvc.Infrastructure.Security;

const string HstsValue = "max-age=31536000; includeSubDomains; preload";

static bool ShouldSendHsts(HttpContext ctx)
{
    if (!ctx.Request.IsHttps) return false;
    var host = (ctx.Request.Host.Host ?? string.Empty).ToLowerInvariant();
    // 避免把本機釘住（HSTS 會被瀏覽器記住）
    if (host is "localhost" or "127.0.0.1" or "::1") return false;
    return true;
}

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration, true);
builder.Services.AddWebServices(builder.Configuration, builder.Environment);

// 在反向代理/容器後面，讓 IsHttps 等能正確判斷（X-Forwarded-Proto/For）
builder.Services.Configure<ForwardedHeadersOptions>(o =>
{
    o.ForwardedHeaders = ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedFor;
#if NET10_0_OR_GREATER
    o.KnownIPNetworks.Clear();
#else
    o.KnownNetworks.Clear();
#endif
    o.KnownProxies.Clear();
});

builder.WebHost.ConfigureKestrel(o => o.AddServerHeader = false);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    await app.InitialiseDatabaseAsync();

    // 顯示 Cloudflare 配置
    var cloudflare = builder.Configuration.GetSection("Cloudflare").Get<CloudflareOptions>();
    Console.WriteLine($"SiteKey = {cloudflare.SiteKey}, SecretKey = {cloudflare.SecretKey}, SiteVerify = {cloudflare.SiteVerify}");
}

// 例外處理頁（所有環境）
app.UseExceptionHandler("/Error/Index");

// 需在最前面，讓後續 IsHttps 判斷正確
app.UseForwardedHeaders();

// HTTPS 相關
app.UseHttpsRedirection();

// ★ 全域補 HSTS（所有環境），包含開發用資源（如 aspnetcore-browser-refresh.js）
//   但排除 localhost/127.0.0.1/::1，以免本機被 HSTS 釘住造成調試不便。
app.Use(async (ctx, next) =>
{
    ctx.Response.OnStarting(() =>
    {
        if (ShouldSendHsts(ctx) && !ctx.Response.Headers.ContainsKey("Strict-Transport-Security"))
        {
            ctx.Response.Headers["Strict-Transport-Security"] = HstsValue;
        }
        return Task.CompletedTask;
    });

    await next();
});

// ★ 正式環境仍啟用官方 UseHsts（行為更完整；若已存在標頭則不會重覆）
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseCustomizedMiddleware();

app.UseSecurityHeaders(app.Environment);

app.UseHttpsRedirection();
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        // 1 年 + immutable（若檔名帶指紋）
        ctx.Context.Response.Headers["Cache-Control"] = "public, max-age=31536000, immutable";
    }
});

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapFallbackToController("PageNotFound", "Error");

app.Run();

// 單元測試用
public partial class Program
{
    protected Program() { }
}
