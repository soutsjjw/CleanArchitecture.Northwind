using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Features.Inventory.Queries.GetInventoryTransactions;

public sealed record GetInventoryTransactionsQuery
    : IRequest<Result<PaginatedList<InventoryTransactionDto>>>
{
    public int ProductId { get; init; }

    public int PageNumber { get; init; } = 1;

    public int PageSize { get; init; } = 10;
}
