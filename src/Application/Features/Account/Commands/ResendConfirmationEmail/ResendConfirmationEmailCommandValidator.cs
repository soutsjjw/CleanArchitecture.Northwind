namespace CleanArchitecture.Northwind.Application.Features.Account.Commands.ResendConfirmationEmail;

public class ResendConfirmationEmailCommandValidator : AbstractValidator<ResendConfirmationEmailCommand>
{
    public ResendConfirmationEmailCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("電子郵件不可為空")
            .EmailAddress().WithMessage("必須是有效的電子郵件格式");
    }
}
