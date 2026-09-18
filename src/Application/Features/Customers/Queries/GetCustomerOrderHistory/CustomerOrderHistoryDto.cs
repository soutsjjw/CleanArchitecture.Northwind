using CleanArchitecture.Northwind.Application.Common.Models;
using CleanArchitecture.Northwind.Application.Features.Orders.Queries.GetOrders;

namespace CleanArchitecture.Northwind.Application.Features.Customers.Queries.GetCustomerOrderHistory;

public sealed class CustomerOrderHistoryDto
{
    public PaginatedList<CustomerOrderHistoryItemDto> Orders { get; init; } = default!;
}

public sealed class CustomerOrderHistoryItemDto
{
    public int Id { get; init; }

    public DateTime? OrderDate { get; init; }

    public decimal TotalAmount { get; init; }

    public OrderShippingStatus ShippingStatus { get; init; }

    public string ShipperName { get; init; } = string.Empty;
}
