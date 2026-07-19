using CleanArchitecture.Northwind.Application.Common.Interfaces.Identity;

namespace CleanArchitecture.Northwind.Application.Features.Account.Commands.UserRegister;

public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    private readonly IIdentitySettings _identitySettings;

    public RegisterUserCommandValidator(IIdentitySettings identitySettings)
    {
        _identitySettings = identitySettings;

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("{PropertyName}不可為空")
            .EmailAddress().WithMessage("必須是有效的{PropertyName}格式")
            .WithName("電子郵件");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("{PropertyName}不可為空")
            .MinimumLength(_identitySettings.RequiredLength).WithMessage("{PropertyName}長度最少為" + _identitySettings.RequiredLength + "碼")
            .Matches(_identitySettings.RequireDigit ? @"\d" : string.Empty).WithMessage("{PropertyName}必須包含數字")
            .Matches(_identitySettings.RequireNonAlphanumeric ? @"[^\w\d]" : string.Empty).WithMessage("{PropertyName}必須包含非字母數字符")
            .Matches(_identitySettings.RequireUpperCase ? @"[A-Z]" : string.Empty).WithMessage("{PropertyName}必須包含大寫字母")
            .Matches(_identitySettings.RequireLowerCase ? @"[a-z]" : string.Empty).WithMessage("{PropertyName}必須包含小寫字母")
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

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("{PropertyName}不可為空")
            .WithName("姓名");

        RuleFor(x => x.IDNo)
            .Matches(@"^[A-Za-z0-9]+$").WithMessage("ID號碼只能包含字母和數字")
            .When(x => !string.IsNullOrEmpty(x.IDNo))
            .WithName("身分證號");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("{PropertyName}不可為空")
            .WithName("職稱");
    }
}
