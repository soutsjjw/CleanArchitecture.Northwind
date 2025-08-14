namespace CleanArchitecture.Northwind.Application.Features.Role.Queries.GetAccount;

public class GetAccountQueryValidator : AbstractValidator<GetAccountQuery>
{
    public GetAccountQueryValidator()
    {
        RuleFor(x => x.DepartmentId)
            .NotNull().WithMessage("部門ID不可為空");

        RuleFor(x => x.OfficeId)
            .NotNull().WithMessage("單位ID不可為空");
    }
}
