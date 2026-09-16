using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Features.Customers.Queries.GetCustomers;

namespace CleanArchitecture.Northwind.Web.ViewModels.Customers;

public sealed class CustomerIndexViewModel
{
    public string? Keyword { get; init; }
    public string? Country { get; init; }
    public string? City { get; init; }
    public CustomerSortField? SortBy { get; init; }
    public bool SortDescending { get; init; }
    public IPaginatedList Pagination { get; init; } = default!;

    public int TotalCount { get; init; }

    public int FirstItemIndex { get; init; }

    public int LastItemIndex { get; init; }
    public IReadOnlyList<string> Countries { get; init; } = [];
    public IReadOnlyList<string> Cities { get; init; } = [];
    public IReadOnlyList<CustomerListItemViewModel> Items { get; init; } = [];
}

public sealed class CustomerListItemViewModel
{
    public string DetailsProtectedId { get; init; } = string.Empty;
    public string CompanyName { get; init; } = string.Empty;
    public string ContactName { get; init; } = string.Empty;
    public string Country { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
}
