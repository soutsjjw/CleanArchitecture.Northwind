using CleanArchitecture.Northwind.Infrastructure.Configurations;
using CleanArchitecture.Northwind.Infrastructure.Data;
using CleanArchitecture.Northwind.Mvc.StartupExtensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration, true);
builder.Services.AddWebServices(builder.Configuration, builder.Environment);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    await app.InitialiseDatabaseAsync();

    // 顯示 Cloudflare 配置
    var cloudflare = builder.Configuration.GetSection("Cloudflare").Get<CloudflareOptions>();
    Console.WriteLine($"SiteKey = {cloudflare.SiteKey}, SecretKey = {cloudflare.SecretKey}, SiteVerify = {cloudflare.SiteVerify}");
}
//else
//{
//    app.UseExceptionHandler("/Error/Index");
//    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
//    app.UseHsts();
//}

app.UseExceptionHandler("/Error/Index");
// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
app.UseHsts();

app.UseCustomizedMiddleware();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapFallbackToController("PageNotFound", "Error");

app.Run();
