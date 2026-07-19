using CleanArchitecture.Northwind.Web.Infrastructure.Security;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace CleanArchitecture.Northwind.Application.FunctionalTests.Infrastructure;

public class SecurityHeadersExtensionsTests
{
    [Test]
    public async Task UseSecurityHeadersAllowsExistingInlineStyleAttributes()
    {
        var csp = await BuildContentSecurityPolicyAsync(Environments.Production);

        csp.ShouldContain("style-src 'self' 'nonce-");
        csp.ShouldNotContain("'unsafe-inline' https://cdn.jsdelivr.net");
        csp.ShouldContain("style-src-attr 'unsafe-inline'");
    }

    [Test]
    public async Task UseSecurityHeadersAllowsDevelopmentBrowserToolingConnectionsOnlyInDevelopment()
    {
        var developmentCsp = await BuildContentSecurityPolicyAsync(Environments.Development);
        var productionCsp = await BuildContentSecurityPolicyAsync(Environments.Production);

        developmentCsp.ShouldContain("connect-src 'self' http://localhost:* https://localhost:* ws://localhost:* wss://localhost:* http://127.0.0.1:* https://127.0.0.1:* ws://127.0.0.1:* wss://127.0.0.1:*");
        productionCsp.ShouldContain("connect-src 'self'");
        productionCsp.ShouldNotContain("http://localhost:*");
        productionCsp.ShouldNotContain("ws://localhost:*");
    }

    private static async Task<string> BuildContentSecurityPolicyAsync(string environmentName)
    {
        var services = new ServiceCollection().BuildServiceProvider();
        var app = new ApplicationBuilder(services);
        var environment = Mock.Of<IWebHostEnvironment>(x => x.EnvironmentName == environmentName);

        app.UseSecurityHeaders(environment);
        app.Run(context => context.Response.WriteAsync("ok"));

        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await app.Build().Invoke(context);

        return context.Response.Headers.ContentSecurityPolicy.ToString();
    }
}
