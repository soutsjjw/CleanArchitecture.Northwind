namespace CleanArchitecture.Northwind.Application.Features.Member.Commands.ChangePassword;

public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("目前密碼不可為空");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("新密碼不可為空")
            .MinimumLength(12).WithMessage("新密碼長度最少為12碼")
            .Matches(@"\d").WithMessage("新密碼必須包含數字")
            .Matches(@"[^\w\d]").WithMessage("新密碼必須包含非字母數字符")
            .Matches(@"[A-Z]").WithMessage("新密碼必須包含大寫字母")
            .Matches(@"[a-z]").WithMessage("新密碼必須包含小寫字母");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("確認新密碼不可為空")
            .Equal(x => x.NewPassword).WithMessage("確認新密碼必須與新密碼相同");
    }
}
