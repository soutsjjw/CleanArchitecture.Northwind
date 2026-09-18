using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Features.Customers.Queries.GetCustomerOrderHistory;

public sealed record GetCustomerOrderHistoryQuery(string CustomerId)
    : IRequest<Result<CustomerOrderHistoryDto>>
{
    public int PageNumber { get; init; } = 1;

    public int PageSize { get; init; } = 10;
}
