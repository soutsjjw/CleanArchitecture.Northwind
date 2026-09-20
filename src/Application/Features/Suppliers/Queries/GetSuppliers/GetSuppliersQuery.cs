using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Features.Suppliers.Queries.GetSuppliers;

public sealed record GetSuppliersQuery : IRequest<Result<PaginatedList<SupplierListItemDto>>>
{
    public string? Keyword { get; init; }

    public bool? IsActive { get; init; }

    public SupplierSortField? SortBy { get; init; }

    public bool SortDescending { get; init; }

    public int PageNumber { get; init; } = 1;

    public int PageSize { get; init; } = 10;
}

public enum SupplierSortField
{
    CompanyName,
    ContactName,
    Phone,
    Country,
    IsActive,
    ProductCount
}
