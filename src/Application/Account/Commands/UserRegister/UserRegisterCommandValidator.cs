namespace CleanArchitecture.Northwind.Application.Account.Commands.UserRegister;

public class UserRegisterCommandValidator : AbstractValidator<UserRegisterCommand>
{
    public UserRegisterCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("{PropertyName}不可為空")
            .EmailAddress().WithMessage("必須是有效的{PropertyName}格式")
            .WithName("電子郵件");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("{PropertyName}不可為空")
            .MinimumLength(12).WithMessage("{PropertyName}長度最少為12碼")
            .Matches(@"\d").WithMessage("{PropertyName}必須包含數字")
            .Matches(@"[^\w\d]").WithMessage("{PropertyName}必須包含非字母數字符")
            .Matches(@"[A-Z]").WithMessage("{PropertyName}必須包含大寫字母")
            .Matches(@"[a-z]").WithMessage("{PropertyName}必須包含小寫字母")
            .WithName("密碼")
            .Custom((password, context) =>
            {
                if (!string.IsNullOrEmpty(password))
                {
                    var uniqueChars = password.Distinct().Count();
                    if (uniqueChars < 4)
                    {
                        context.AddFailure($"{context.DisplayName}必須包含至少 4 個唯一字符");
                    }
                }
            });
    }
}
