using CleanArchitecture.Northwind.Application.Common.Interfaces;

namespace CleanArchitecture.Northwind.Web.ViewModels.Products;

public sealed class ProductIndexViewModel
{
    public string? Keyword { get; init; }

    public int? CategoryId { get; init; }

    public int? SupplierId { get; init; }

    public bool? Discontinued { get; init; }

    public bool LowStockOnly { get; init; }

    public IPaginatedList Pagination { get; init; } = default!;

    public IReadOnlyList<ProductListItemViewModel> Items { get; init; } = [];

    public IReadOnlyList<ProductOptionViewModel> Categories { get; init; } = [];

    public IReadOnlyList<ProductOptionViewModel> Suppliers { get; init; } = [];
}

public sealed class ProductListItemViewModel
{
    public int Id { get; init; }

    public string DetailsProtectedId { get; init; } = string.Empty;

    public string ImageProtectedId { get; init; } = string.Empty;

    public string EditProtectedId { get; init; } = string.Empty;

    public string DeleteProtectedId { get; init; } = string.Empty;

    public string SetDiscontinuedProtectedId { get; init; } = string.Empty;

    public string ProductName { get; init; } = string.Empty;

    public string CategoryName { get; init; } = string.Empty;

    public string SupplierName { get; init; } = string.Empty;

    public decimal UnitPrice { get; init; }

    public short UnitsInStock { get; init; }

    public short ReorderLevel { get; init; }

    public bool Discontinued { get; init; }

    public bool HasPicture { get; init; }
}

public sealed record ProductOptionViewModel(
    int Id,
    string Name);
