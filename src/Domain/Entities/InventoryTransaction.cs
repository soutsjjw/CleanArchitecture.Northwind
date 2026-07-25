namespace CleanArchitecture.Northwind.Domain.Entities;

public sealed class InventoryTransaction : BaseAuditableEntity<int>
{
    public int ProductId { get; set; }

    public InventoryTransactionType TransactionType { get; set; }

    public short QuantityBefore { get; set; }

    public short QuantityDelta { get; set; }

    public short QuantityAfter { get; set; }

    public string Reason { get; set; } = null!;

    public string? SourceDocumentType { get; set; }

    public int? SourceDocumentId { get; set; }

    public Product Product { get; set; } = null!;
}
