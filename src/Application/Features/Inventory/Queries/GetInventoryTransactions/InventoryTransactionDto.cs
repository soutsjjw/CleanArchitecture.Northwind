using CleanArchitecture.Northwind.Domain.Enums;

namespace CleanArchitecture.Northwind.Application.Features.Inventory.Queries.GetInventoryTransactions;

public sealed record InventoryTransactionDto(
    int Id,
    InventoryTransactionType TransactionType,
    short QuantityBefore,
    short QuantityDelta,
    short QuantityAfter,
    string Reason,
    DateTimeOffset Created,
    string CreatedBy);
