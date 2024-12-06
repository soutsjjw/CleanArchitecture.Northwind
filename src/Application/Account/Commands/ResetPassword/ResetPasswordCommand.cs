using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Account.Commands.ResetPassword;

public record ResetPasswordCommand : IRequest<Result>
{
    public required string Email { get; init; }

    public required string ResetCode { get; init; }

    public required string NewPassword { get; init; }
}
