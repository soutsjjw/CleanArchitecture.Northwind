using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Features.Products.Commands.DeleteProduct;

public sealed record DeleteProductCommand : IRequest<Result>
{
    public int Id { get; init; }
}

public sealed class DeleteProductCommandValidator : AbstractValidator<DeleteProductCommand>
{
    public DeleteProductCommandValidator()
    {
        RuleFor(command => command.Id)
            .GreaterThan(0)
            .WithMessage("商品編號無效。");
    }
}

public sealed class DeleteProductCommandHandler(IApplicationDbContext context)
    : IRequestHandler<DeleteProductCommand, Result>
{
    public async Task<Result> Handle(
        DeleteProductCommand request,
        CancellationToken cancellationToken)
    {
        var product = await context.Products.FindAsync([request.Id], cancellationToken);
        if (product is null || product.IsDelete)
        {
            return Result.Failure("找不到商品。", 404);
        }

        var hasOrderHistory = await context.OrderDetails
            .AnyAsync(detail => detail.ProductId == request.Id, cancellationToken);
        if (hasOrderHistory)
        {
            return Result.Failure("商品已有訂單紀錄，請改為停用。", 409);
        }

        var hasInventoryHistory = await context.InventoryTransactions
            .AnyAsync(
                transaction => transaction.ProductId == request.Id,
                cancellationToken);
        if (hasInventoryHistory)
        {
            return Result.Failure("商品已有庫存異動紀錄，請改為停用。", 409);
        }

        product.IsDelete = true;
        product.Discontinued = true;
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
