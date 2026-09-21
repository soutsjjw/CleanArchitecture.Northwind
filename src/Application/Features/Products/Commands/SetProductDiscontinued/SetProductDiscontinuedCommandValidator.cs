namespace CleanArchitecture.Northwind.Application.Features.Products.Commands.SetProductDiscontinued;

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
