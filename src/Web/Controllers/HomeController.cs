using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CleanArchitecture.Northwind.Application.Features.Dashboard.Queries.GetOperationsDashboard;
using CleanArchitecture.Northwind.Application.Features.Products.Queries.GetPublicProducts;
using CleanArchitecture.Northwind.Web.Services;
using CleanArchitecture.Northwind.Web.ViewModels.Home;

namespace CleanArchitecture.Northwind.Web.Controllers;

public class HomeController : BaseController<HomeController>
{
    public HomeController(ILogger<HomeController> logger)
        : base(logger)
    {
    }

    [HttpGet]
    [AllowAnonymous]
    public Task<IActionResult> Index(CancellationToken cancellationToken = default)
        => GetPublicCatalogAsync(
            keyword: null,
            categoryId: null,
            pageNumber: 1,
            pageSize: 10,
            cancellationToken);

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> Index(
        string? keyword = null,
        int? categoryId = null,
        int pageNumber = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
        => GetPublicCatalogAsync(
            keyword,
            categoryId,
            pageNumber,
            pageSize,
            cancellationToken);

    private async Task<IActionResult> GetPublicCatalogAsync(
        string? keyword,
        int? categoryId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetPublicProductsQuery
        {
            Keyword = keyword,
            CategoryId = categoryId,
            PageNumber = Math.Max(pageNumber, 1),
            PageSize = pageSize is 10 or 20 or 50 or 100 ? pageSize : 10
        }, cancellationToken);
        return result.Succeeded
            ? View(MapCatalog(result.Data, keyword, categoryId))
            : RedirectToAction("Index", "Error");
    }

    [Authorize(Policy = "Orders:Read:All")]
    [Authorize(Policy = "Products:Read:All")]
    public async Task<IActionResult> Dashboard(CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetOperationsDashboardQuery(), cancellationToken);
        return result.Succeeded
            ? View(Map(result.Data))
            : RedirectToAction("Index", "Error");
    }

    [HttpGet]
    [Authorize(Policy = "Orders:Read:All")]
    [Authorize(Policy = "Products:Read:All")]
    public async Task<IActionResult> DashboardData(CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetOperationsDashboardQuery(), cancellationToken);
        return result.Succeeded
            ? Json(Map(result.Data))
            : Problem(statusCode: StatusCodes.Status500InternalServerError, title: "無法取得營運儀表板資料。");
    }

    [HttpGet]
    [AllowAnonymous]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> ProductImage(
        int id,
        CancellationToken cancellationToken = default)
    {
        var result = await Mediator.Send(
            new GetPublicProductImageQuery(id),
            cancellationToken);
        if (!result.Succeeded
            || !ProductImageValidator.TryValidateStored(
                result.Data.Picture,
                result.Data.PictureContentType,
                out var image))
        {
            return NotFound();
        }

        return File(image!.Content, image.ContentType);
    }

    [AllowAnonymous]
    public IActionResult Privacy()
    {
        return View();
    }

    [AllowAnonymous]
    public IActionResult TermsOfService()
    {
        return View();
    }

    private static OperationsDashboardViewModel Map(OperationsDashboardDto dto) => new()
    {
        TodayOrderCount = dto.TodayOrderCount,
        TodayRevenue = dto.TodayRevenue,
        MonthlyOrderCount = dto.MonthlyOrderCount,
        MonthlyRevenue = dto.MonthlyRevenue,
        ShippingStatuses = dto.ShippingStatuses.Select(x => new ShippingStatusViewModel(x.Status, x.Count)).ToList(),
        LowStockProducts = dto.LowStockProducts.Select(x => new LowStockProductViewModel(x.ProductName, x.UnitsInStock, x.ReorderLevel)).ToList(),
        TopCustomers = dto.TopCustomers.Select(x => new TopCustomerViewModel(x.CompanyName, x.OrderCount, x.Revenue)).ToList()
    };

    private static PublicProductCatalogViewModel MapCatalog(
        PublicProductCatalogDto catalog,
        string? keyword,
        int? categoryId) => new()
    {
        Keyword = keyword,
        CategoryId = categoryId,
        Pagination = catalog.Products,
        Items = catalog.Products.Items.Select(product =>
            new PublicProductCardViewModel(
                product.Id,
                product.ProductName,
                product.CategoryName,
                product.UnitPrice,
                product.HasPicture)).ToList(),
        Categories = catalog.Categories.Select(category =>
            new PublicProductCategoryViewModel(category.Id, category.Name)).ToList()
    };
}
