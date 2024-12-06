namespace CleanArchitecture.Northwind.Application.Features.Account.Commands.UserLogin;

public class UserLoginCommandValidator : AbstractValidator<UserLoginCommand>
{
    public UserLoginCommandValidator()
    {
        RuleFor(x => x.UserName)
                .NotEmpty().WithMessage("帳號是必填的");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("密碼是必填的");
    }
}
