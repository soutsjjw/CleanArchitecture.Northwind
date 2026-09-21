namespace CleanArchitecture.Northwind.Application.Features.Categories.Commands.CreateCategory;

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
