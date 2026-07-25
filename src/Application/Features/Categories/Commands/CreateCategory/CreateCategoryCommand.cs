using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Common.Models;
using CleanArchitecture.Northwind.Application.Features.Categories.Commands;
using CleanArchitecture.Northwind.Domain.Entities;

namespace CleanArchitecture.Northwind.Application.Features.Categories.Commands.CreateCategory;

public sealed record CreateCategoryCommand : IRequest<Result<int>>
{
    public string CategoryName { get; init; } = string.Empty;

    public string? Description { get; init; }
}

public sealed class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(command => command.CategoryName)
            .Must(name => !string.IsNullOrWhiteSpace(name))
            .WithMessage("分類名稱不可為空。")
            .Must(name => name is null || name.Trim().Length <= 15)
            .WithMessage("分類名稱不可超過 15 個字元。");
    }
}

public sealed class CreateCategoryCommandHandler(IApplicationDbContext context)
    : IRequestHandler<CreateCategoryCommand, Result<int>>
{
    public async Task<Result<int>> Handle(
        CreateCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var inputError = CategoryCommandSupport.ValidateInput(request.CategoryName);
        if (inputError is not null)
        {
            return Result<int>.Failure(inputError, 400);
        }

        var category = new Category
        {
            CategoryName = request.CategoryName.Trim(),
            Description = CategoryCommandSupport.TrimToNull(request.Description),
            IsActive = true
        };

        context.Categories.Add(category);
        await context.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(category.Id);
    }
}
