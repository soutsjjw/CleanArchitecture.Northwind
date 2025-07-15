using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Features.Account.Commands.ForgotPassword;

public record ForgotPasswordCommand : IRequest<Result>
{
    public string Email { get; init; }

    public string Link { get; init; }
}
