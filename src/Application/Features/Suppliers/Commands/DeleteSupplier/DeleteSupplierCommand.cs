using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Features.Suppliers.Commands.DeleteSupplier;

public sealed record DeleteSupplierCommand : IRequest<Result>
{
    public int Id { get; init; }
}

public sealed class DeleteSupplierCommandValidator : AbstractValidator<DeleteSupplierCommand>
{
    public DeleteSupplierCommandValidator()
    {
        RuleFor(command => command.Id)
            .GreaterThan(0)
            .WithMessage("供應商編號無效。");
    }
}

public sealed class DeleteSupplierCommandHandler(IApplicationDbContext context)
    : IRequestHandler<DeleteSupplierCommand, Result>
{
    public async Task<Result> Handle(
        DeleteSupplierCommand request,
        CancellationToken cancellationToken)
    {
        if (request.Id <= 0)
        {
            return Result.Failure("供應商編號無效。", 400);
        }

        var supplier = await context.Suppliers.FindAsync([request.Id], cancellationToken);
        if (supplier is null || supplier.IsDelete)
        {
            return Result.Failure("找不到供應商。", 404);
        }

        var hasProducts = await context.Products.AnyAsync(
            product => product.SupplierId == request.Id,
            cancellationToken);
        if (hasProducts)
        {
            return Result.Failure("供應商已有商品使用，請改為停用。", 409);
        }

        supplier.IsDelete = true;
        supplier.IsActive = false;
        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
