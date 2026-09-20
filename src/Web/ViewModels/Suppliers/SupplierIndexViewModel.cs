using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Features.Suppliers.Queries.GetSuppliers;

namespace CleanArchitecture.Northwind.Web.ViewModels.Suppliers;

public sealed class SupplierIndexViewModel
{
    public string? Keyword { get; init; }
    public bool? IsActive { get; init; }
    public SupplierSortField? SortBy { get; init; }
    public bool SortDescending { get; init; }
    public IPaginatedList Pagination { get; init; } = default!;
    public IReadOnlyList<SupplierListItemViewModel> Items { get; init; } = [];
}

public sealed class SupplierListItemViewModel
{
    public string DetailsProtectedId { get; init; } = string.Empty;
    public string EditProtectedId { get; init; } = string.Empty;
    public string DeleteProtectedId { get; init; } = string.Empty;
    public string SetActiveProtectedId { get; init; } = string.Empty;
    public string CompanyName { get; init; } = string.Empty;
    public string? ContactName { get; init; }
    public string? Phone { get; init; }
    public string? Country { get; init; }
    public bool IsActive { get; init; }
    public int ProductCount { get; init; }
}
