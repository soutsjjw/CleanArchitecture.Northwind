namespace CleanArchitecture.Northwind.Application.Account.Commands.ResetPassword;

public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("電子郵件不可為空")
            .EmailAddress().WithMessage("必須是有效的電子郵件格式");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("新密碼不可為空")
            .MinimumLength(12).WithMessage("新密碼長度最少為12碼")
            .Matches(@"\d").WithMessage("新密碼必須包含數字")
            .Matches(@"[^\w\d]").WithMessage("新密碼必須包含非字母數字符")
            .Matches(@"[A-Z]").WithMessage("新密碼必須包含大寫字母")
            .Matches(@"[a-z]").WithMessage("新密碼必須包含小寫字母");
    }
}
