using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Features.Categories.Commands.DeleteCategory;

public sealed record DeleteCategoryCommand : IRequest<Result>
{
    public int Id { get; init; }
}

public sealed class DeleteCategoryCommandValidator : AbstractValidator<DeleteCategoryCommand>
{
    public DeleteCategoryCommandValidator()
    {
        RuleFor(command => command.Id)
            .GreaterThan(0)
            .WithMessage("分類編號無效。");
    }
}

public sealed class DeleteCategoryCommandHandler(IApplicationDbContext context)
    : IRequestHandler<DeleteCategoryCommand, Result>
{
    public async Task<Result> Handle(
        DeleteCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var category = await context.Categories.FindAsync([request.Id], cancellationToken);
        if (category is null || category.IsDelete)
        {
            return Result.Failure("找不到分類。", 404);
        }

        var hasProducts = await context.Products
            .AnyAsync(product => product.CategoryId == request.Id, cancellationToken);
        if (hasProducts)
        {
            return Result.Failure("分類已有商品使用，請改為停用。", 409);
        }

        category.IsDelete = true;
        category.IsActive = false;
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
