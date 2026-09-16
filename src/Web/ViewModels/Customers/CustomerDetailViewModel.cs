using CleanArchitecture.Northwind.Application.Features.Customers.Queries.GetCustomers;

namespace CleanArchitecture.Northwind.Web.ViewModels.Customers;

public sealed class CustomerDetailViewModel
{
    public string Id { get; init; } = string.Empty;
    public string CompanyName { get; init; } = string.Empty;
    public string? ContactName { get; init; }
    public string? ContactTitle { get; init; }
    public string? Address { get; init; }
    public string? City { get; init; }
    public string? Region { get; init; }
    public string? PostalCode { get; init; }
    public string? Country { get; init; }
    public string? Phone { get; init; }
    public string? Fax { get; init; }
    public string? Keyword { get; init; }
    public string? CountryFilter { get; init; }
    public string? CityFilter { get; init; }
    public CustomerSortField? SortBy { get; init; }
    public bool SortDescending { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
}
