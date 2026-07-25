using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Common.Models;
using CleanArchitecture.Northwind.Application.Features.Categories.Commands;

namespace CleanArchitecture.Northwind.Application.Features.Categories.Commands.UpdateCategory;

public sealed record UpdateCategoryCommand : IRequest<Result>
{
    public int Id { get; init; }

    public string CategoryName { get; init; } = string.Empty;

    public string? Description { get; init; }
}

public sealed class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryCommandValidator()
    {
        RuleFor(command => command.Id)
            .GreaterThan(0)
            .WithMessage("分類編號無效。");

        RuleFor(command => command.CategoryName)
            .Must(name => !string.IsNullOrWhiteSpace(name))
            .WithMessage("分類名稱不可為空。")
            .Must(name => name is null || name.Trim().Length <= 15)
            .WithMessage("分類名稱不可超過 15 個字元。");
    }
}

public sealed class UpdateCategoryCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateCategoryCommand, Result>
{
    public async Task<Result> Handle(
        UpdateCategoryCommand request,
        CancellationToken cancellationToken)
    {
        if (request.Id <= 0)
        {
            return Result.Failure("分類編號無效。", 400);
        }

        var inputError = CategoryCommandSupport.ValidateInput(request.CategoryName);
        if (inputError is not null)
        {
            return Result.Failure(inputError, 400);
        }

        var category = await context.Categories.FindAsync([request.Id], cancellationToken);
        if (category is null || category.IsDelete)
        {
            return Result.Failure("找不到分類。", 404);
        }

        category.CategoryName = request.CategoryName.Trim();
        category.Description = CategoryCommandSupport.TrimToNull(request.Description);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
