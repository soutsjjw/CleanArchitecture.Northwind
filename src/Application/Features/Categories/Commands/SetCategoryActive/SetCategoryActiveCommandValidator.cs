namespace CleanArchitecture.Northwind.Application.Features.Categories.Commands.SetCategoryActive;

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
