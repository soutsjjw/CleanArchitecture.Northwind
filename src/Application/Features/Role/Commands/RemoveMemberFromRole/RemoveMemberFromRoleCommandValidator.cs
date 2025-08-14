namespace CleanArchitecture.Northwind.Application.Features.Role.Commands.RemoveMemberFromRole;

public class RemoveMemberFromRoleCommandValidator : AbstractValidator<RemoveMemberFromRoleCommand>
{
    public RemoveMemberFromRoleCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotNull().WithMessage("帳號資料不可為空");

        RuleFor(x => x.RoleId)
            .NotNull().WithMessage("角色資料不可為空");
    }
}
