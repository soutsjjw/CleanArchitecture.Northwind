namespace CleanArchitecture.Northwind.Application.Features.Suppliers.Commands.DeleteSupplier;

public sealed class DeleteSupplierCommandValidator : AbstractValidator<DeleteSupplierCommand>
{
    public DeleteSupplierCommandValidator()
    {
        RuleFor(command => command.Id)
            .GreaterThan(0)
            .WithMessage("供應商編號無效。");
    }
}
