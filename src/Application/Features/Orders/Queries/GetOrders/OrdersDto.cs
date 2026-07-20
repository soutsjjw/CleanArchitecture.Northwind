using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Features.Orders.Queries.GetOrders;

public class OrdersDto
{
    public string? Keyword { get; init; }

    public DateTime? OrderedFrom { get; init; }

    public DateTime? OrderedTo { get; init; }

    public OrderShippingStatus? ShippingStatus { get; init; }

    public PaginatedList<OrderItemDto> Orders { get; init; } = default!;
}

public class OrderItemDto
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

public enum OrderShippingStatus
{
    All = 0,
    Unshipped = 1,
    Shipped = 2,
    Overdue = 3
}
