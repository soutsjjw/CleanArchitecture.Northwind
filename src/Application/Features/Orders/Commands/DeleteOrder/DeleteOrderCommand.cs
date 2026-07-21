using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Features.Orders.Commands.DeleteOrder;

public record DeleteOrderCommand : IRequest<Result>
{
    public int Id { get; init; }
}
