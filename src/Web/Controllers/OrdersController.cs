using System.Globalization;
using CleanArchitecture.Northwind.Application.Common.Interfaces;
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
    private readonly IDataProtectionService _dataProtectionService;

    public OrdersController(
        IMapper mapper,
        IDataProtectionService dataProtectionService,
        ILogger<OrdersController> logger)
        : base(logger)
    {
        _mapper = mapper;
        _dataProtectionService = dataProtectionService;
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
    public async Task<IActionResult> Details(string id)
    {
        if (!TryUnprotectOrderId(id, out var orderId))
        {
            return RedirectToAction(nameof(Index)).WithError(this, "訂單識別碼無效。");
        }

        var result = await Mediator.Send(new GetOrderDetailQuery { Id = orderId });
        if (!result.Succeeded)
        {
            return RedirectToAction(nameof(Index)).WithError(this, result.Errors.ToList());
        }

        return View(MapToDetailViewModel(result.Data));
    }

    [HttpGet]
    [Authorize(Policy = Policies.Orders_Delete)]
    public async Task<IActionResult> DeleteConfirmation(string id)
    {
        if (!TryUnprotectOrderId(id, out var orderId))
        {
            return NotFound();
        }

        var result = await Mediator.Send(new GetOrderDetailQuery { Id = orderId });
        if (!result.Succeeded)
        {
            return NotFound();
        }

        return PartialView("_DeleteConfirmationModal", MapToDetailViewModel(result.Data));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = Policies.Orders_Delete)]
    public async Task<IActionResult> Delete(string id)
    {
        if (!TryUnprotectOrderId(id, out var orderId))
        {
            return RedirectToAction(nameof(Index)).WithError(this, "訂單識別碼無效。");
        }

        var result = await Mediator.Send(new DeleteOrderCommand { Id = orderId });
        var redirect = RedirectToAction(nameof(Index));

        return result.Succeeded
            ? redirect.WithSuccess(this, "訂單已刪除。")
            : redirect.WithError(this, result.Errors.ToList());
    }

    private OrderIndexViewModel MapToViewModel(OrdersDto dto)
    {
        var viewModel = _mapper.Map<OrderIndexViewModel>(new
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

        return new OrderIndexViewModel
        {
            Keyword = viewModel.Keyword,
            OrderedFrom = viewModel.OrderedFrom,
            OrderedTo = viewModel.OrderedTo,
            ShippingStatus = viewModel.ShippingStatus,
            SortBy = viewModel.SortBy,
            SortDescending = viewModel.SortDescending,
            Pagination = viewModel.Pagination,
            TotalCount = viewModel.TotalCount,
            FirstItemIndex = viewModel.FirstItemIndex,
            LastItemIndex = viewModel.LastItemIndex,
            Items = viewModel.Items.Select(item => new OrderItemViewModel
            {
                Id = item.Id,
                ProtectedId = ProtectOrderId(item.Id),
                CustomerId = item.CustomerId,
                CustomerName = item.CustomerName,
                EmployeeName = item.EmployeeName,
                OrderDate = item.OrderDate,
                RequiredDate = item.RequiredDate,
                ShippedDate = item.ShippedDate,
                ShipperName = item.ShipperName,
                Freight = item.Freight,
                ShipCity = item.ShipCity,
                ShipCountry = item.ShipCountry,
                LineCount = item.LineCount,
                TotalAmount = item.TotalAmount,
                ShippingStatus = item.ShippingStatus
            }).ToList()
        };
    }

    private OrderDetailViewModel MapToDetailViewModel(OrderDetailDto dto)
    {
        var viewModel = _mapper.Map<OrderDetailViewModel>(dto);

        return new OrderDetailViewModel
        {
            Id = viewModel.Id,
            ProtectedId = ProtectOrderId(viewModel.Id),
            CustomerId = viewModel.CustomerId,
            CustomerName = viewModel.CustomerName,
            EmployeeName = viewModel.EmployeeName,
            OrderDate = viewModel.OrderDate,
            RequiredDate = viewModel.RequiredDate,
            ShippedDate = viewModel.ShippedDate,
            ShipperName = viewModel.ShipperName,
            Freight = viewModel.Freight,
            ShipName = viewModel.ShipName,
            ShipAddress = viewModel.ShipAddress,
            ShipCity = viewModel.ShipCity,
            ShipRegion = viewModel.ShipRegion,
            ShipPostalCode = viewModel.ShipPostalCode,
            ShipCountry = viewModel.ShipCountry,
            Items = viewModel.Items.Select(item => new OrderLineItemViewModel
            {
                ProductName = item.ProductName,
                UnitPrice = item.UnitPrice,
                Quantity = item.Quantity,
                Discount = item.Discount,
                TotalAmount = item.TotalAmount
            }).ToList()
        };
    }

    private bool TryUnprotectOrderId(string id, out int orderId)
    {
        orderId = default;

        if (string.IsNullOrWhiteSpace(id))
        {
            return false;
        }

        var unprotectedId = _dataProtectionService.Unprotect(id);

        return int.TryParse(
            unprotectedId,
            NumberStyles.Integer,
            CultureInfo.InvariantCulture,
            out orderId);
    }

    private string ProtectOrderId(int orderId)
        => _dataProtectionService.Protect(orderId.ToString(CultureInfo.InvariantCulture));
}
