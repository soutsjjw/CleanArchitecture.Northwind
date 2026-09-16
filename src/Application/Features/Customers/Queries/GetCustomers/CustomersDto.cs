using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Features.Customers.Queries.GetCustomers;

public enum CustomerSortField
{
    CompanyName,
    ContactName,
    Country,
    City,
    Phone
}

public sealed record CustomerListItemDto(
    string Id,
    string CompanyName,
    string ContactName,
    string Country,
    string City,
    string Phone);

public sealed class CustomersDto
{
    public string? Keyword { get; init; }

    public string? Country { get; init; }

    public string? City { get; init; }

    public CustomerSortField? SortBy { get; init; }

    public bool SortDescending { get; init; }

    public IReadOnlyList<string> Countries { get; init; } = [];

    public IReadOnlyList<string> Cities { get; init; } = [];

    public PaginatedList<CustomerListItemDto> Customers { get; init; } = default!;
}