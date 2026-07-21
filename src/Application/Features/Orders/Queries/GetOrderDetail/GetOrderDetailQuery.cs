using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Features.Orders.Queries.GetOrderDetail;

public record GetOrderDetailQuery : IRequest<Result<OrderDetailDto>>
{
    public int Id { get; init; }
}
