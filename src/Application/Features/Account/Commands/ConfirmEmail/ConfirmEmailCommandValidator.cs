namespace CleanArchitecture.Northwind.Application.Features.Account.Commands.ConfirmEmail;

public class ConfirmEmailCommandValidator : AbstractValidator<ConfirmEmailCommand>
{
    public ConfirmEmailCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("電子郵件不可為空")
            .EmailAddress().WithMessage("必須是有效的電子郵件格式");

        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("令牌不可為空");
    }
}
