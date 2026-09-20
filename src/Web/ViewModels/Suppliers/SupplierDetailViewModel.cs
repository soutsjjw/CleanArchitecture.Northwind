using CleanArchitecture.Northwind.Application.Features.Suppliers.Queries.GetSuppliers;

namespace CleanArchitecture.Northwind.Web.ViewModels.Suppliers;

public sealed class SupplierDetailViewModel
{
    public string? Keyword { get; init; }
    public bool? IsActiveFilter { get; init; }
    public SupplierSortField? SortBy { get; init; }
    public bool SortDescending { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string DetailsProtectedId { get; init; } = string.Empty;
    public string EditProtectedId { get; init; } = string.Empty;
    public string DeleteProtectedId { get; init; } = string.Empty;
    public string SetActiveProtectedId { get; init; } = string.Empty;
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
    public string? HomePage { get; init; }
    public bool IsActive { get; init; }
    public int ProductCount { get; init; }
    public IReadOnlyList<SupplierSuppliedProductViewModel> Products { get; init; } = [];
}

public sealed class SupplierSuppliedProductViewModel
{
    public string DetailsProtectedId { get; init; } = string.Empty;
    public string ProductName { get; init; } = string.Empty;
    public string? QuantityPerUnit { get; init; }
    public decimal? UnitPrice { get; init; }
    public bool Discontinued { get; init; }
}
