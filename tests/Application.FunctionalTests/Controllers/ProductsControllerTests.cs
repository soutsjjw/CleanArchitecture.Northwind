using System.Reflection;
using CleanArchitecture.Northwind.Application.Common.Models;
using CleanArchitecture.Northwind.Application.Features.Products.Queries.GetProductDetail;
using CleanArchitecture.Northwind.Application.Features.Products.Queries.GetProductFormOptions;
using CleanArchitecture.Northwind.Domain.Constants;
using CleanArchitecture.Northwind.Web.Controllers;
using CleanArchitecture.Northwind.Web.ViewModels.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;

// This controller-only suite intentionally lives outside the Application
// functional-test SetUpFixture namespace so it does not start SQL Testcontainers.
namespace CleanArchitecture.Northwind.Web.FunctionalTests.Controllers;

public class ProductsControllerTests
{
    [Test]
    public async Task ProductEditShouldIncludeItsExistingInactiveSupplier()
    {
        var sender = new Mock<ISender>();
        sender.Setup(value => value.Send(
                It.IsAny<GetProductDetailQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ProductDetailDto>.Success(new ProductDetailDto(
                7, "測試商品", 3, "飲料", 5, "已停用供應商", null, 20,
                0, 0, 0, false, [1], null, null)));
        sender.Setup(value => value.Send(
                It.IsAny<GetProductFormOptionsQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ProductFormOptionsDto>.Success(
                new ProductFormOptionsDto(
                    [new ProductFormOptionDto(3, "飲料")],
                    [new ProductFormOptionDto(6, "啟用供應商")])));
        var authorization = new Mock<IAuthorizationService>();
        authorization.Setup(value => value.AuthorizeAsync(
                It.IsAny<System.Security.Claims.ClaimsPrincipal>(),
                It.IsAny<object?>(),
                Policies.Suppliers_Read))
            .ReturnsAsync(AuthorizationResult.Success());
        var provider = new EphemeralDataProtectionProvider();
        var controller = new ProductsController(
            sender.Object,
            provider,
            authorization.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
        var protectedId = provider.CreateProtector("Products.Edit.ItemId.v1")
            .Protect("7");

        var result = await controller.Edit(protectedId, CancellationToken.None);

        var view = result.ShouldBeOfType<ViewResult>();
        var model = view.Model.ShouldBeOfType<ProductEditViewModel>();
        model.Suppliers.ShouldContain(
            supplier => supplier.Id == 5 && supplier.Name == "已停用供應商");
    }

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
