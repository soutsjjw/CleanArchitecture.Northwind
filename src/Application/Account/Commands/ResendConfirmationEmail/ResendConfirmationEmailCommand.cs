using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Account.Commands.ResendConfirmationEmail;

public record ResendConfirmationEmailCommand : IRequest<Result>
{
    public string Email { get; set; }
}

