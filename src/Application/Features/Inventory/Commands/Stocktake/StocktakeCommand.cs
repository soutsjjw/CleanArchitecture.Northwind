using CleanArchitecture.Northwind.Application.Common.Models;
using CleanArchitecture.Northwind.Application.Features.Inventory.Commands.AdjustInventory;

namespace CleanArchitecture.Northwind.Application.Features.Inventory.Commands.Stocktake;

public sealed record StocktakeCommand(
    int ProductId,
    short ActualQuantity,
    string Reason,
    byte[] RowVersion) : IRequest<Result<InventoryCommandResult>>;
