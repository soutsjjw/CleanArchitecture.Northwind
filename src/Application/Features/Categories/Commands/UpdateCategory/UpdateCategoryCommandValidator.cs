namespace CleanArchitecture.Northwind.Application.Features.Categories.Commands.UpdateCategory;

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
