using CleanArchitecture.Northwind.Infrastructure.Data;
using CleanArchitecture.Northwind.WebAPI.Middleware;
using CleanArchitecture.Northwind.WebAPI.StartupExtensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddKeyVaultIfConfigured(builder.Configuration);

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddWebServices(builder.Configuration, builder.Environment);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    await app.InitialiseDatabaseAsync();
}
else
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

HealthCheckExtension.UseCustomizedHealthCheck(app, builder.Configuration, builder.Environment);
app.UseMiddleware<HealthCheckIpRestrictionMiddleware>(builder.Configuration);

app.UseHttpsRedirection();
app.UseStaticFiles();

app.MapControllers();

app.UseAuthentication();
app.UseAuthorization();

app.UseCustomizedSwagger(app.Environment);

app.Run();

// 單元測試用
public partial class Program { }
