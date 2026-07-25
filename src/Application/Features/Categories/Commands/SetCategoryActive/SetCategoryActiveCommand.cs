using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Features.Categories.Commands.SetCategoryActive;

public sealed record SetCategoryActiveCommand : IRequest<Result>
{
    public int Id { get; init; }

    public bool IsActive { get; init; }
}

public sealed class SetCategoryActiveCommandValidator
    : AbstractValidator<SetCategoryActiveCommand>
{
    public SetCategoryActiveCommandValidator()
    {
        RuleFor(command => command.Id)
            .GreaterThan(0)
            .WithMessage("分類編號無效。");
    }
}

public sealed class SetCategoryActiveCommandHandler(IApplicationDbContext context)
    : IRequestHandler<SetCategoryActiveCommand, Result>
{
    public async Task<Result> Handle(
        SetCategoryActiveCommand request,
        CancellationToken cancellationToken)
    {
        var category = await context.Categories.FindAsync([request.Id], cancellationToken);
        if (category is null || category.IsDelete)
        {
            return Result.Failure("找不到分類。", 404);
        }

        category.IsActive = request.IsActive;
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
