using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Common.Models;
using CleanArchitecture.Northwind.Application.Features.Inventory.Commands.AdjustInventory;
using CleanArchitecture.Northwind.Domain.Enums;

namespace CleanArchitecture.Northwind.Application.Features.Inventory.Commands.Stocktake;

public sealed class StocktakeCommandHandler(IApplicationDbContext context)
    : IRequestHandler<StocktakeCommand, Result<InventoryCommandResult>>
{
    public Task<Result<InventoryCommandResult>> Handle(
        StocktakeCommand request,
        CancellationToken cancellationToken)
    {
        return InventoryCommandExecutor.Execute(
            context,
            request.ProductId,
            request.Reason,
            request.RowVersion,
            InventoryTransactionType.Stocktake,
            _ => request.ActualQuantity,
            cancellationToken);
    }
}
