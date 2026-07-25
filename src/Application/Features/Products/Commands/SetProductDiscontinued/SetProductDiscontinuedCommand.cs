using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Features.Products.Commands.SetProductDiscontinued;

public sealed record SetProductDiscontinuedCommand : IRequest<Result>
{
    public int Id { get; init; }

    public bool Discontinued { get; init; }
}

public sealed class SetProductDiscontinuedCommandValidator
    : AbstractValidator<SetProductDiscontinuedCommand>
{
    public SetProductDiscontinuedCommandValidator()
    {
        RuleFor(command => command.Id)
            .GreaterThan(0)
            .WithMessage("商品編號無效。");
    }
}

public sealed class SetProductDiscontinuedCommandHandler(IApplicationDbContext context)
    : IRequestHandler<SetProductDiscontinuedCommand, Result>
{
    public async Task<Result> Handle(
        SetProductDiscontinuedCommand request,
        CancellationToken cancellationToken)
    {
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
