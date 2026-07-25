using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Features.Inventory.Commands.AdjustInventory;

public sealed record AdjustInventoryCommand(
    int ProductId,
    short QuantityDelta,
    string Reason,
    byte[] RowVersion) : IRequest<Result<InventoryCommandResult>>;

public sealed record InventoryCommandResult(
    short UnitsInStock,
    byte[] RowVersion);
