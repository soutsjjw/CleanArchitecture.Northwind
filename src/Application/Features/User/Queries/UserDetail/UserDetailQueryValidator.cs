namespace CleanArchitecture.Northwind.Application.Features.User.Queries.UserDetail;

public class UserDetailQueryValidator : AbstractValidator<UserDetailQuery>
{
    public UserDetailQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotNull().WithMessage("使用者ID不可為空");
    }
}
