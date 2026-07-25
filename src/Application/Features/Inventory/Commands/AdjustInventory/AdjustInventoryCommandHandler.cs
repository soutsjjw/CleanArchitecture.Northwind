using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Common.Models;
using CleanArchitecture.Northwind.Domain.Entities;
using CleanArchitecture.Northwind.Domain.Enums;

namespace CleanArchitecture.Northwind.Application.Features.Inventory.Commands.AdjustInventory;

public sealed class AdjustInventoryCommandHandler(IApplicationDbContext context)
    : IRequestHandler<AdjustInventoryCommand, Result<InventoryCommandResult>>
{
    public Task<Result<InventoryCommandResult>> Handle(
        AdjustInventoryCommand request,
        CancellationToken cancellationToken)
    {
        if (request.QuantityDelta == 0)
        {
            return Task.FromResult(
                Result<InventoryCommandResult>.Failure("庫存調整數量不可為 0。"));
        }

        return InventoryCommandExecutor.Execute(
            context,
            request.ProductId,
            request.Reason,
            request.RowVersion,
            InventoryTransactionType.ManualAdjustment,
            quantityBefore => quantityBefore + request.QuantityDelta,
            cancellationToken);
    }
}

internal static class InventoryCommandExecutor
{
    internal static async Task<Result<InventoryCommandResult>> Execute(
        IApplicationDbContext context,
        int productId,
        string reason,
        byte[] rowVersion,
        InventoryTransactionType transactionType,
        Func<short, int> calculateQuantityAfter,
        CancellationToken cancellationToken)
    {
        if (productId <= 0)
        {
            return Result<InventoryCommandResult>.Failure("商品編號無效。");
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            return Result<InventoryCommandResult>.Failure("調整原因不可為空。");
        }

        var trimmedReason = reason.Trim();
        if (trimmedReason.Length > 250)
        {
            return Result<InventoryCommandResult>.Failure("調整原因不可超過 250 個字元。");
        }

        if (rowVersion is not { Length: > 0 })
        {
            return Result<InventoryCommandResult>.Failure("庫存版本不可為空。");
        }

        var product = await context.Products.FindAsync([productId], cancellationToken);
        if (product is null || product.Discontinued || product.IsDelete)
        {
            return Result<InventoryCommandResult>.Failure("商品不存在或已停用。");
        }

        var quantityBefore = product.UnitsInStock ?? 0;
        var quantityAfterValue = calculateQuantityAfter(quantityBefore);
        if (quantityAfterValue < 0)
        {
            return Result<InventoryCommandResult>.Failure("庫存不足，無法完成操作。");
        }

        if (quantityAfterValue > short.MaxValue)
        {
            return Result<InventoryCommandResult>.Failure("庫存數量超出允許範圍。");
        }

        var quantityDeltaValue = quantityAfterValue - quantityBefore;
        if (quantityDeltaValue is < short.MinValue or > short.MaxValue)
        {
            return Result<InventoryCommandResult>.Failure("庫存異動數量超出允許範圍。");
        }

        var quantityAfter = checked((short)quantityAfterValue);
        var quantityDelta = checked((short)quantityDeltaValue);
        var transaction = new InventoryTransaction
        {
            ProductId = product.Id,
            TransactionType = transactionType,
            QuantityBefore = quantityBefore,
            QuantityDelta = quantityDelta,
            QuantityAfter = quantityAfter,
            Reason = trimmedReason
        };

        product.UnitsInStock = quantityAfter;
        context.PrepareInventoryUpdate(product, rowVersion.ToArray());
        context.InventoryTransactions.Add(transaction);

        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            product.UnitsInStock = quantityBefore;
            context.InventoryTransactions.Remove(transaction);
            return Result<InventoryCommandResult>.Failure("庫存已被其他使用者更新");
        }

        return Result<InventoryCommandResult>.Success(
            new InventoryCommandResult(
                quantityAfter,
                product.RowVersion.ToArray()));
    }
}
