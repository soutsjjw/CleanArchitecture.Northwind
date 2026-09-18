using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Features.Customers.Queries.GetCustomers;

public sealed record GetCustomersQuery : IRequest<Result<CustomersDto>>
{
    public string? Keyword { get; init; }

    public string? Country { get; init; }

    public string? City { get; init; }

    public CustomerSortField? SortBy { get; init; }

    public bool SortDescending { get; init; }

    public int PageNumber { get; init; } = 1;

    public int PageSize { get; init; } = 10;
}
