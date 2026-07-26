using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Domain.Enums;

namespace CleanArchitecture.Northwind.Web.ViewModels.Products;

public sealed class ProductDetailViewModel
{
    public int Id { get; init; }

    public string ProtectedId { get; init; } = string.Empty;

    public string ProductName { get; init; } = string.Empty;

    public string CategoryName { get; init; } = string.Empty;

    public string SupplierName { get; init; } = string.Empty;

    public string? QuantityPerUnit { get; init; }

    public decimal UnitPrice { get; init; }

    public short UnitsInStock { get; init; }

    public short UnitsOnOrder { get; init; }

    public short ReorderLevel { get; init; }

    public bool Discontinued { get; init; }

    public bool HasPicture { get; init; }

    public string RowVersion { get; init; } = string.Empty;

    public IPaginatedList? InventoryPagination { get; init; }

    public IReadOnlyList<InventoryTransactionItemViewModel> InventoryTransactions { get; init; } = [];
}

public sealed class InventoryTransactionItemViewModel
{
    public InventoryTransactionType TransactionType { get; init; }

    public short QuantityDelta { get; init; }

    public short QuantityBefore { get; init; }

    public short QuantityAfter { get; init; }

    public string? SourceDocumentType { get; init; }

    public int? SourceDocumentId { get; init; }

    public string Reason { get; init; } = string.Empty;

    public string CreatedBy { get; init; } = string.Empty;

    public DateTimeOffset Created { get; init; }
}
