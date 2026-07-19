using CleanArchitecture.Northwind.Infrastructure.Data;
using CleanArchitecture.Northwind.Web.Infrastructure.Security;
using CleanArchitecture.Northwind.Web.StartupExtensions;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

builder.AddApplicationServices();
builder.AddInfrastructureServices(true);
builder.AddWebServices();

// Trust proxy headers before middleware reads Request.Scheme or remote IP.
builder.Services.Configure<ForwardedHeadersOptions>(o =>
{
    o.ForwardedHeaders = ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedFor;
    o.KnownIPNetworks.Clear();
    o.KnownProxies.Clear();
});

builder.WebHost.ConfigureKestrel(o => o.AddServerHeader = false);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    await app.InitialiseDatabaseAsync();
}

// Forwarded headers must run before HSTS, HTTPS redirection, and security headers.
app.UseForwardedHeaders();

app.UseExceptionHandler("/Error/Index");

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseSecurityHeaders(app.Environment);

app.UseCustomizedMiddleware();

app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        // 1 year + immutable for fingerprinted static assets.
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

// For integration tests.
public partial class Program
{
    protected Program() { }
}
