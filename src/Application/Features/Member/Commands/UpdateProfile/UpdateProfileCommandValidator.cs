namespace CleanArchitecture.Northwind.Application.Features.Account.Commands.UpdateProfile;

public class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator()
    {
        RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("姓名是必填的");

        RuleFor(x => x.Title)
                .NotEmpty().WithMessage("職稱是必填的");
    }
}
