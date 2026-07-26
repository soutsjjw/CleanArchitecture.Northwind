using System.Reflection;
using System.Security.Claims;
using CleanArchitecture.Northwind.Application.Common.Interfaces.Identity;
using CleanArchitecture.Northwind.Application.Common.Models;
using CleanArchitecture.Northwind.Domain.Constants;
using CleanArchitecture.Northwind.Infrastructure.Authorization;
using CleanArchitecture.Northwind.Web.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

// This authorization-only suite intentionally lives outside the Application
// functional-test SetUpFixture namespace so it does not start SQL Testcontainers.
namespace CleanArchitecture.Northwind.Web.FunctionalTests.Controllers;

public class InventoryAuthorizationTests
{
    [Test]
    public async Task Inventory_create_policy_forbids_user_without_permission()
    {
        await using var provider = CreateAuthorizationProvider([]);
        var authorization = provider.GetRequiredService<IAuthorizationService>();
        var user = new ClaimsPrincipal(
            new ClaimsIdentity(
                [new Claim(ClaimTypes.NameIdentifier, "user-without-inventory")],
                "test"));

        var result = await authorization.AuthorizeAsync(
            user,
            Policies.Inventory_Create);

        result.Succeeded.ShouldBeFalse();
    }

    [Test]
    public async Task Inventory_policies_accept_matching_all_scope_permissions()
    {
        PermissionParser.TryParse(
            "Inventory:Read:All",
            out var readPermission).ShouldBeTrue();
        PermissionParser.TryParse(
            "Inventory:Create:All",
            out var createPermission).ShouldBeTrue();

        await using var provider = CreateAuthorizationProvider(
        [
            readPermission,
            createPermission
        ]);
        var authorization = provider.GetRequiredService<IAuthorizationService>();
        var user = new ClaimsPrincipal(
            new ClaimsIdentity(
                [new Claim(ClaimTypes.NameIdentifier, "inventory-user")],
                "test"));

        (await authorization.AuthorizeAsync(user, Policies.Inventory_Read))
            .Succeeded.ShouldBeTrue();
        (await authorization.AuthorizeAsync(user, Policies.Inventory_Create))
            .Succeeded.ShouldBeTrue();
    }

    [Test]
    public void Inventory_actions_use_policy_constants_and_posts_require_anti_forgery()
    {
        AssertActionPolicy(
            nameof(ProductsController.Details),
            Policies.Inventory_Read,
            requireAntiForgery: false);
        AssertActionPolicy(
            nameof(ProductsController.AdjustInventory),
            Policies.Inventory_Create);
        AssertActionPolicy(
            nameof(ProductsController.Stocktake),
            Policies.Inventory_Create);

        var controllerSource = File.ReadAllText(Path.Combine(
            GetRepositoryRoot(),
            "src",
            "Web",
            "Controllers",
            "ProductsController.cs"));
        var detailsView = File.ReadAllText(Path.Combine(
            GetRepositoryRoot(),
            "src",
            "Web",
            "Views",
            "Products",
            "Details.cshtml"));

        controllerSource.ShouldNotContain("[Authorize(Policy = \"Inventory:");
        detailsView.ShouldNotContain("AuthorizeAsync(User, \"Inventory:");
    }

    [Test]
    public void Role_permission_modules_and_sidebar_cover_phase_one_dependencies()
    {
        Modules.All.ShouldContain(Modules.Products);
        Modules.All.ShouldContain(Modules.Categories);
        Modules.All.ShouldContain(Modules.Suppliers);
        Modules.All.ShouldContain(Modules.Inventory);

        var sidebar = File.ReadAllText(Path.Combine(
            GetRepositoryRoot(),
            "src",
            "Web",
            "Views",
            "Shared",
            "_SideBarPartial.cshtml"));

        sidebar.ShouldContain("Policies.Suppliers_Read");
        sidebar.ShouldContain("canViewSuppliers.Succeeded");
        sidebar.ShouldContain(
            "canViewProducts.Succeeded && canViewSuppliers.Succeeded");
    }

    private static ServiceProvider CreateAuthorizationProvider(
        IReadOnlyList<Permission> permissions)
    {
        var resolver = new Mock<IPermissionResolver>();
        resolver
            .Setup(value => value.GetForUserAsync(
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(permissions);

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAuthorization();
        services.AddSingleton<IAuthorizationPolicyProvider,
            PermissionPolicyProvider>();
        services.AddSingleton<IAuthorizationHandler,
            PermissionAuthorizationHandler>();
        services.AddSingleton(resolver.Object);
        return services.BuildServiceProvider();
    }

    private static void AssertActionPolicy(
        string actionName,
        string expectedPolicy,
        bool requireAntiForgery = true)
    {
        var methods = typeof(ProductsController)
            .GetMethods()
            .Where(method => method.Name == actionName)
            .ToArray();

        methods.ShouldNotBeEmpty();
        methods
            .SelectMany(method =>
                method.GetCustomAttributes<AuthorizeAttribute>())
            .Select(attribute => attribute.Policy)
            .ShouldContain(expectedPolicy);

        if (requireAntiForgery)
        {
            methods
                .Where(method =>
                    method.GetCustomAttribute<HttpPostAttribute>() is not null)
                .ShouldAllBe(method =>
                    method.GetCustomAttribute<ValidateAntiForgeryTokenAttribute>()
                    != null);
        }
    }

    private static string GetRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null
               && !File.Exists(Path.Combine(
                   directory.FullName,
                   "CleanArchitecture.Northwind.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName
            ?? throw new DirectoryNotFoundException();
    }
}
