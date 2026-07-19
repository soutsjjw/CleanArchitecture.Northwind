namespace CleanArchitecture.Northwind.Application.Features.Totp.Commands.EnableTotp;

public class EnableTotpCommandValidator : AbstractValidator<EnableTotpCommand>
{
    public EnableTotpCommandValidator()
    {
        RuleFor(x => x.Code)
                .NotEmpty().WithMessage("驗證碼是必填的");
    }
}
