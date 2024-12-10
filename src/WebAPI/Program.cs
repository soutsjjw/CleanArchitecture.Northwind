using CleanArchitecture.Northwind.Infrastructure.Data;
using CleanArchitecture.Northwind.WebAPI.StartupExtensions;
using Microsoft.AspNetCore.Authentication;

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
app.UseCustomizedMiddleware();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.MapControllers();

app.UseAuthentication();

//app.Use(async (context, next) =>
//{
//    if (context.User.Identity.IsAuthenticated)
//    {
//        Console.WriteLine($"User {context.User.Identity.Name} is authenticated.");
//    }
//    else
//    {
//        Console.WriteLine("User is not authenticated.");
//    }
//    await next();
//});

app.Use(async (context, next) =>
{
    var schemeProvider = context.RequestServices.GetRequiredService<IAuthenticationSchemeProvider>();
    var schemes = await schemeProvider.GetAllSchemesAsync();

    Console.WriteLine("Registered Authentication Schemes:");
    foreach (var scheme in schemes)
    {
        Console.WriteLine($" - {scheme.Name}");
    }

    await next();
});

//app.Use(async (context, next) =>
//{
//    var authenticateResult = await context.AuthenticateAsync();
//    Console.WriteLine($"Authenticated Scheme: {authenticateResult?.Ticket?.AuthenticationScheme ?? "None"}");
//    await next();
//});

app.UseAuthorization();

app.UseCustomizedSwagger(app.Environment);

app.Run();

// 單元測試用
public partial class Program { }
