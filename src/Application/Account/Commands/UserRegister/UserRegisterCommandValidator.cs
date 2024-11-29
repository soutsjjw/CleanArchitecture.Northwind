namespace CleanArchitecture.Northwind.Application.Account.Commands.UserRegister;

public class UserRegisterCommandValidator : AbstractValidator<UserRegisterCommand>
{
    public UserRegisterCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email 不可為空")
            .EmailAddress().WithMessage("必須是有效的 Email 格式");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password 不可為空")
            .MinimumLength(12).WithMessage("Password 長度最少為12碼")
            .Matches(@"\d").WithMessage("Password 必須包含數字")
            .Matches(@"[^\w\d]").WithMessage("Password 必須包含非字母數字符")
            .Matches(@"[A-Z]").WithMessage("Password 必須包含大寫字母")
            .Matches(@"[a-z]").WithMessage("Password 必須包含小寫字母");
    }
}
