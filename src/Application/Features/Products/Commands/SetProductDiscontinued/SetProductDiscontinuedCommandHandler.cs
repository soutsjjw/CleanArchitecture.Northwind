using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Features.Products.Commands.SetProductDiscontinued;

public sealed class SetProductDiscontinuedCommandHandler(IApplicationDbContext context)
    : IRequestHandler<SetProductDiscontinuedCommand, Result>
{
    public async Task<Result> Handle(
        SetProductDiscontinuedCommand request,
        CancellationToken cancellationToken)
    {
        if (request.Id <= 0)
        {
            return Result.Failure("商品編號無效。", 400);
        }

        var product = await context.Products.FindAsync([request.Id], cancellationToken);
        if (product is null || product.IsDelete)
        {
            return Result.Failure("找不到商品。", 404);
        }

        product.Discontinued = request.Discontinued;
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
