namespace CleanArchitecture.Northwind.Application.Features.Role.Queries.EditRolePrepare;

public class EditRolePrepareQueryValidator : AbstractValidator<EditRolePrepareQuery>
{
    public EditRolePrepareQueryValidator()
    {
        RuleFor(x => x.RoleId)
            .NotNull().WithMessage("角色ID不可為空");
    }
}
