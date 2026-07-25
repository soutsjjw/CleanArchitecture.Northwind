using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Features.Inventory.Queries.GetInventoryTransactions;

public sealed class GetInventoryTransactionsQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetInventoryTransactionsQuery, Result<PaginatedList<InventoryTransactionDto>>>
{
    public async Task<Result<PaginatedList<InventoryTransactionDto>>> Handle(
        GetInventoryTransactionsQuery request,
        CancellationToken cancellationToken)
    {
        var query = context.InventoryTransactions
            .AsNoTracking()
            .Where(transaction => transaction.ProductId == request.ProductId)
            .OrderByDescending(transaction => transaction.Created)
            .ThenByDescending(transaction => transaction.Id)
            .Select(transaction => new InventoryTransactionDto(
                transaction.Id,
                transaction.TransactionType,
                transaction.QuantityBefore,
                transaction.QuantityDelta,
                transaction.QuantityAfter,
                transaction.Reason,
                transaction.Created,
                transaction.CreatedBy));

        var transactions = await PaginatedList<InventoryTransactionDto>.CreateAsync(
            query,
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        return Result<PaginatedList<InventoryTransactionDto>>.Success(transactions);
    }
}
