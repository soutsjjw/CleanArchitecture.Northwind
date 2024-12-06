using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Account.Commands.UserRegister;

public record UserRegisterCommand : IRequest<Result>
{
    public required string Email { get; init; }

    public required string Password { get; init; }

    public string ConfirmationLink { get; set; } = "";
}
