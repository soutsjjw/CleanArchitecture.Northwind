using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Features.Orders.Queries.GetOrders;

public record GetOrdersQuery : IRequest<Result<OrdersDto>>
{
    public int PageNumber { get; init; } = 1;

    public int PageSize { get; init; } = 10;

    public string? Keyword { get; init; }

    public DateTime? OrderedFrom { get; init; }

    public DateTime? OrderedTo { get; init; }

    public OrderShippingStatus? ShippingStatus { get; init; }
}
