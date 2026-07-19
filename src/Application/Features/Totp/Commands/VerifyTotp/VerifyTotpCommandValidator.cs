namespace CleanArchitecture.Northwind.Application.Features.Totp.Commands.VerifyTotp;

public class VerifyTotpCommandValidator : AbstractValidator<VerifyTotpCommand>
{
    public VerifyTotpCommandValidator()
    {
        RuleFor(x => x.Code)
                .NotEmpty().WithMessage("驗證碼是必填的");
    }
}
