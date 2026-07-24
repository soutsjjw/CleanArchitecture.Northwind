using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CleanArchitecture.Northwind.Application.Features.Dashboard.Queries.GetOperationsDashboard;
using CleanArchitecture.Northwind.Web.ViewModels.Home;

namespace CleanArchitecture.Northwind.Web.Controllers;

public class HomeController : BaseController<HomeController>
{
    public HomeController(ILogger<HomeController> logger)
        : base(logger)
    {
    }

    [Authorize(Policy = "Orders:Read:All")]
    [Authorize(Policy = "Products:Read:All")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
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
}
