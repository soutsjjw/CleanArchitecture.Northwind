namespace CleanArchitecture.Northwind.Application.Features.Account.Commands.UserLogin;

public class UserLoginCommandValidator : AbstractValidator<UserLoginCommand>
{
    public UserLoginCommandValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("帳號是必填的")
            .EmailAddress().WithMessage("帳號必須是有效的 Email 格式");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("密碼是必填的");
    }
}
