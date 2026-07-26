using System.Reflection;
using CleanArchitecture.Northwind.Domain.Constants;
using CleanArchitecture.Northwind.Web.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// This controller-only suite intentionally lives outside the Application
// functional-test SetUpFixture namespace so it does not start SQL Testcontainers.
namespace CleanArchitecture.Northwind.Web.FunctionalTests.Controllers;

public class CategoriesControllerTests
{
    [Test]
    public void Category_actions_require_scoped_policies_and_mutations_require_anti_forgery()
    {
        AssertActionPolicy(nameof(CategoriesController.Index), Policies.Categories_Read, false);
        AssertActionPolicy(nameof(CategoriesController.Create), Policies.Categories_Create);
        AssertActionPolicy(nameof(CategoriesController.Edit), Policies.Categories_Update);
        AssertActionPolicy(nameof(CategoriesController.Delete), Policies.Categories_Delete);
        AssertActionPolicy(nameof(CategoriesController.SetActive), Policies.Categories_Update);
    }

    [Test]
    public void Category_views_only_transmit_protected_ids_for_existing_resources()
    {
        var index = File.ReadAllText(GetWebViewPath("Index.cshtml"));
        var form = File.ReadAllText(GetWebViewPath("_CategoryForm.cshtml"));

        index.ShouldContain("asp-route-id=\"@category.EditProtectedId\"");
        index.ShouldContain("value=\"@category.SetActiveProtectedId\"");
        index.ShouldContain("value=\"@category.DeleteProtectedId\"");
        form.ShouldContain("asp-for=\"ProtectedId\"");
        form.ShouldNotContain("name=\"Id\"");
    }

    [Test]
    public void Sidebar_links_are_guarded_by_product_and_category_read_policies()
    {
        var source = File.ReadAllText(Path.Combine(
            GetRepositoryRoot(), "src", "Web", "Views", "Shared", "_SideBarPartial.cshtml"));

        source.ShouldContain("Policies.Products_Read");
        source.ShouldContain("Policies.Categories_Read");
        source.ShouldContain("canViewProducts.Succeeded");
        source.ShouldContain("canViewCategories.Succeeded");
    }

    private static void AssertActionPolicy(
        string actionName,
        string expectedPolicy,
        bool requireAntiForgery = true)
    {
        var methods = typeof(CategoriesController)
            .GetMethods()
            .Where(method => method.Name == actionName)
            .ToArray();

        methods.ShouldNotBeEmpty();
        methods
            .SelectMany(method => method.GetCustomAttributes<AuthorizeAttribute>())
            .Select(attribute => attribute.Policy)
            .ShouldContain(expectedPolicy);

        if (requireAntiForgery)
        {
            methods
                .Where(method => method.GetCustomAttribute<HttpPostAttribute>() is not null)
                .ShouldAllBe(method =>
                    method.GetCustomAttribute<ValidateAntiForgeryTokenAttribute>() != null);
        }
    }

    private static string GetWebViewPath(string fileName)
        => Path.Combine(GetRepositoryRoot(), "src", "Web", "Views", "Categories", fileName);

    private static string GetRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null
               && !File.Exists(Path.Combine(directory.FullName, "CleanArchitecture.Northwind.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new DirectoryNotFoundException();
    }
}
