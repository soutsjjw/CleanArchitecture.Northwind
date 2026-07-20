using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Features.Orders.Queries.GetOrders;

namespace CleanArchitecture.Northwind.Web.ViewModels.Orders;

public class OrderIndexViewModel
{
    public string? Keyword { get; init; }

    public DateTime? OrderedFrom { get; init; }

    public DateTime? OrderedTo { get; init; }

    public OrderShippingStatus? ShippingStatus { get; init; }

    public IPaginatedList Pagination { get; init; } = default!;

    public int TotalCount { get; init; }

    public int FirstItemIndex { get; init; }

    public int LastItemIndex { get; init; }

    public IReadOnlyList<OrderItemViewModel> Items { get; init; } = Array.Empty<OrderItemViewModel>();
}

public class OrderItemViewModel
{
    public int Id { get; init; }

    public string? CustomerId { get; init; }

    public string CustomerName { get; init; } = string.Empty;

    public string EmployeeName { get; init; } = string.Empty;

    public DateTime? OrderDate { get; init; }

    public DateTime? RequiredDate { get; init; }

    public DateTime? ShippedDate { get; init; }

    public string ShipperName { get; init; } = string.Empty;

    public decimal Freight { get; init; }

    public string ShipCity { get; init; } = string.Empty;

    public string ShipCountry { get; init; } = string.Empty;

    public int LineCount { get; init; }

    public decimal TotalAmount { get; init; }

    public OrderShippingStatus ShippingStatus { get; init; }
}
