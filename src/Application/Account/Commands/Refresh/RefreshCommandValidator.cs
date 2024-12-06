namespace CleanArchitecture.Northwind.Application.Account.Commands.Refresh;

public class RefreshCommandValidator : AbstractValidator<RefreshCommand>
{
    public RefreshCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
                .NotEmpty().WithMessage("令牌是必填的");
    }
}
