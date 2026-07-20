using CleanArchitecture.Northwind.Application.Features.Orders.Queries.GetOrders;
using CleanArchitecture.Northwind.Domain.Constants;
using CleanArchitecture.Northwind.Web.ViewModels.Orders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitecture.Northwind.Web.Controllers;

[Authorize(Policy = Policies.Orders)]
public class OrdersController : BaseController<OrdersController>
{
    public OrdersController(ILogger<OrdersController> logger)
        : base(logger)
    {
    }

    [HttpGet]
    [Authorize(Policy = Policies.Orders_Read)]
    public async Task<IActionResult> Index(
        string? keyword,
        DateTime? orderedFrom,
        DateTime? orderedTo,
        OrderShippingStatus? shippingStatus,
        int pageNumber = 1,
        int pageSize = 10)
    {
        var result = await Mediator.Send(new GetOrdersQuery
        {
            Keyword = keyword,
            OrderedFrom = orderedFrom,
            OrderedTo = orderedTo,
            ShippingStatus = shippingStatus,
            PageNumber = pageNumber,
            PageSize = pageSize
        });

        if (!result.Succeeded)
        {
            return RedirectToAction("Index", "Home");
        }

        return View(MapToViewModel(result.Data));
    }

    private static OrderIndexViewModel MapToViewModel(OrdersDto dto)
    {
        return new OrderIndexViewModel
        {
            Keyword = dto.Keyword,
            OrderedFrom = dto.OrderedFrom,
            OrderedTo = dto.OrderedTo,
            ShippingStatus = dto.ShippingStatus,
            Pagination = dto.Orders,
            TotalCount = dto.Orders.TotalCount,
            FirstItemIndex = dto.Orders.FirstItemIndex,
            LastItemIndex = dto.Orders.LastItemIndex,
            Items = dto.Orders.Items
                .Select(order => new OrderItemViewModel
                {
                    Id = order.Id,
                    CustomerId = order.CustomerId,
                    CustomerName = order.CustomerName,
                    EmployeeName = order.EmployeeName,
                    OrderDate = order.OrderDate,
                    RequiredDate = order.RequiredDate,
                    ShippedDate = order.ShippedDate,
                    ShipperName = order.ShipperName,
                    Freight = order.Freight,
                    ShipCity = order.ShipCity,
                    ShipCountry = order.ShipCountry,
                    LineCount = order.LineCount,
                    TotalAmount = order.TotalAmount,
                    ShippingStatus = order.ShippingStatus
                })
                .ToList()
        };
    }
}
