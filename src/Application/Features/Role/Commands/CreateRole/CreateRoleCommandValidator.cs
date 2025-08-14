namespace CleanArchitecture.Northwind.Application.Features.Role.Commands.CreateRole;

public class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotNull().WithMessage("角色名稱不可為空");

        RuleFor(x => x.Description)
            .NotNull().WithMessage("角色說明不可為空");
    }
}
