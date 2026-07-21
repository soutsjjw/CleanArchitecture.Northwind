using CleanArchitecture.Northwind.Application.Features.Orders.Commands.DeleteOrder;
using CleanArchitecture.Northwind.Application.Features.Orders.Queries.GetOrderDetail;
using CleanArchitecture.Northwind.Application.Features.Orders.Queries.GetOrders;
using CleanArchitecture.Northwind.Domain.Constants;
using CleanArchitecture.Northwind.Web.Extensions;
using CleanArchitecture.Northwind.Web.ViewModels.Orders;
using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitecture.Northwind.Web.Controllers;

[Authorize(Policy = Policies.Orders)]
public class OrdersController : BaseController<OrdersController>
{
    private readonly IMapper _mapper;

    public OrdersController(IMapper mapper, ILogger<OrdersController> logger)
        : base(logger)
    {
        _mapper = mapper;
    }

    [HttpGet]
    [Authorize(Policy = Policies.Orders_Read)]
    public Task<IActionResult> Index()
        => GetIndexAsync(new GetOrdersQuery());

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = Policies.Orders_Read)]
    public Task<IActionResult> Index(
        string? keyword,
        DateTime? orderedFrom,
        DateTime? orderedTo,
        OrderShippingStatus? shippingStatus,
        OrderSortField? sortBy,
        bool sortDescending,
        int pageNumber = 1,
        int pageSize = 10)
        => GetIndexAsync(new GetOrdersQuery
        {
            Keyword = keyword,
            OrderedFrom = orderedFrom,
            OrderedTo = orderedTo,
            ShippingStatus = shippingStatus,
            SortBy = sortBy,
            SortDescending = sortDescending,
            PageNumber = pageNumber,
            PageSize = pageSize
        });

    private async Task<IActionResult> GetIndexAsync(GetOrdersQuery query)
    {
        var result = await Mediator.Send(query);

        if (!result.Succeeded)
        {
            return RedirectToAction("Index", "Home");
        }

        return View(MapToViewModel(result.Data));
    }

    [HttpGet]
    [Authorize(Policy = Policies.Orders_Read)]
    public async Task<IActionResult> Details(int id)
    {
        var result = await Mediator.Send(new GetOrderDetailQuery { Id = id });
        if (!result.Succeeded)
        {
            return RedirectToAction(nameof(Index)).WithError(this, result.Errors.ToList());
        }

        return View(MapToDetailViewModel(result.Data));
    }

    [HttpGet]
    [Authorize(Policy = Policies.Orders_Delete)]
    public async Task<IActionResult> DeleteConfirmation(int id)
    {
        var result = await Mediator.Send(new GetOrderDetailQuery { Id = id });
        if (!result.Succeeded)
        {
            return NotFound();
        }

        return PartialView("_DeleteConfirmationModal", MapToDetailViewModel(result.Data));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = Policies.Orders_Delete)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await Mediator.Send(new DeleteOrderCommand { Id = id });
        var redirect = RedirectToAction(nameof(Index));

        return result.Succeeded
            ? redirect.WithSuccess(this, "訂單已刪除。")
            : redirect.WithError(this, result.Errors.ToList());
    }

    private OrderIndexViewModel MapToViewModel(OrdersDto dto)
    {
        return _mapper.Map<OrderIndexViewModel>(new
        {
            dto.Keyword,
            dto.OrderedFrom,
            dto.OrderedTo,
            dto.ShippingStatus,
            dto.SortBy,
            dto.SortDescending,
            Pagination = dto.Orders,
            TotalCount = dto.Orders.TotalCount,
            FirstItemIndex = dto.Orders.FirstItemIndex,
            LastItemIndex = dto.Orders.LastItemIndex,
            Items = dto.Orders.Items
        });
    }

    private OrderDetailViewModel MapToDetailViewModel(OrderDetailDto dto)
        => _mapper.Map<OrderDetailViewModel>(dto);
}
