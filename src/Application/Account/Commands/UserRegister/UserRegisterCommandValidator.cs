namespace CleanArchitecture.Northwind.Application.Account.Commands.UserRegister;

public class UserRegisterCommandValidator : AbstractValidator<UserRegisterCommand>
{
    public UserRegisterCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("電子郵件不可為空")
            .EmailAddress().WithMessage("必須是有效的電子郵件格式");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("密碼不可為空")
            .MinimumLength(12).WithMessage("密碼長度最少為12碼")
            .Matches(@"\d").WithMessage("密碼必須包含數字")
            .Matches(@"[^\w\d]").WithMessage("密碼必須包含非字母數字符")
            .Matches(@"[A-Z]").WithMessage("密碼必須包含大寫字母")
            .Matches(@"[a-z]").WithMessage("密碼必須包含小寫字母");
    }
}
