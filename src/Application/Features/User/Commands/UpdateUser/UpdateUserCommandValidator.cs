namespace CleanArchitecture.Northwind.Application.Features.User.Commands.UpdateUser;

public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotNull().WithMessage("使用者ID不可為空");

        RuleFor(x => x.FullName)
            .NotNull().WithMessage("姓名不可為空");

        RuleFor(x => x.Title)
            .NotNull().WithMessage("職稱不可為空");

        RuleFor(x => x.DepartmentId)
            .NotNull().WithMessage("部門不可為空");

        RuleFor(x => x.OfficeId)
            .NotNull().WithMessage("單位不可為空");
    }
}
