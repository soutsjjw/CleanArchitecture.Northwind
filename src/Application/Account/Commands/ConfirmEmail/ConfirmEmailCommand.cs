using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Account.Commands.ConfirmEmail;

public record ConfirmEmailCommand : IRequest<Result>
{
    public string Email { get; set; }

    public string Token { get; set; }
}

