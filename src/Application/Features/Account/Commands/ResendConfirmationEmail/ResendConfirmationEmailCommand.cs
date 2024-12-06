using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Features.Account.Commands.ResendConfirmationEmail;

public record ResendConfirmationEmailCommand : IRequest<Result>
{
    public string Email { get; set; }

    public string ConfirmationLink { get; set; }
}

