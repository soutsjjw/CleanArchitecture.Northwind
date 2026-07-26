using System.Reflection;
using CleanArchitecture.Northwind.Domain.Constants;
using CleanArchitecture.Northwind.Web.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// This controller-only suite intentionally lives outside the Application
// functional-test SetUpFixture namespace so it does not start SQL Testcontainers.
namespace CleanArchitecture.Northwind.Web.FunctionalTests.Controllers;

public class ProductsControllerTests
{
    [Test]
    public void Product_mutating_actions_require_anti_forgery_and_scoped_policies()
    {
        AssertActionPolicy(nameof(ProductsController.Create), Policies.Products_Create);
        AssertActionPolicy(nameof(ProductsController.Edit), Policies.Products_Update);
        AssertActionPolicy(nameof(ProductsController.Delete), Policies.Products_Delete);
        AssertActionPolicy(nameof(ProductsController.SetDiscontinued), Policies.Products_Update);
        AssertActionPolicy(nameof(ProductsController.AdjustInventory), Policies.Inventory_Create);
        AssertActionPolicy(nameof(ProductsController.Stocktake), Policies.Inventory_Create);
    }

    [Test]
    public void Product_read_actions_and_inventory_history_require_scoped_policies()
    {
        AssertActionPolicy(nameof(ProductsController.Index), Policies.Products_Read, requireAntiForgery: false);
        AssertActionPolicy(nameof(ProductsController.Details), Policies.Products_Read, requireAntiForgery: false);
        AssertActionPolicy(nameof(ProductsController.Image), Policies.Products_Read, requireAntiForgery: false);

        var details = typeof(ProductsController)
            .GetMethods()
            .Single(method => method.Name == nameof(ProductsController.Details));

        details.GetCustomAttributes<AuthorizeAttribute>()
            .Select(attribute => attribute.Policy)
            .ShouldContain(Policies.Inventory_Read);
    }

    [Test]
    public void Product_views_submit_protected_ids_row_versions_and_multipart_images()
    {
        var productForm = File.ReadAllText(GetWebViewPath("_ProductForm.cshtml"));
        var index = File.ReadAllText(GetWebViewPath("Index.cshtml"));
        var details = File.ReadAllText(GetWebViewPath("Details.cshtml"));
        var adjust = File.ReadAllText(GetWebViewPath("AdjustInventory.cshtml"));
        var stocktake = File.ReadAllText(GetWebViewPath("Stocktake.cshtml"));

        productForm.ShouldContain("asp-for=\"ProtectedId\"");
        productForm.ShouldContain("asp-for=\"Picture\"");
        productForm.ShouldContain("multipart/form-data");
        index.ShouldContain("asp-route-id=\"@product.DetailsProtectedId\"");
        index.ShouldContain("asp-route-id=\"@product.EditProtectedId\"");
        index.ShouldContain("value=\"@product.DeleteProtectedId\"");
        details.ShouldContain("asp-route-id=\"@Model.EditProtectedId\"");
        details.ShouldContain("asp-route-id=\"@Model.AdjustInventoryProtectedId\"");
        details.ShouldContain("asp-route-id=\"@Model.StocktakeProtectedId\"");
        details.ShouldContain("value=\"@Model.DeleteProtectedId\"");
        adjust.ShouldContain("asp-route-id=\"@Model.DetailsProtectedId\"");
        stocktake.ShouldContain("asp-route-id=\"@Model.DetailsProtectedId\"");
        adjust.ShouldContain("asp-for=\"RowVersion\"");
        stocktake.ShouldContain("asp-for=\"RowVersion\"");
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
        => Path.Combine(GetRepositoryRoot(), "src", "Web", "Views", "Products", fileName);

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
