namespace CleanArchitecture.Northwind.Application.Features.Account.Commands.ForgotPassword;

public class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordCommandValidator()
    {
        RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("電子郵件不可為空")
            .EmailAddress().WithMessage("必須是有效的電子郵件格式");
    }
}
