namespace CleanArchitecture.Northwind.Application.Features.Account.Commands.UserAPILogin;

public class UserAPILoginCommandValidator : AbstractValidator<UserAPILoginCommand>
{
    public UserAPILoginCommandValidator()
    {
        RuleFor(x => x.UserName)
                .NotEmpty().WithMessage("帳號是必填的");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("密碼是必填的");
    }
}
