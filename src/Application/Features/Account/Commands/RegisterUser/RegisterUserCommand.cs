using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Features.Account.Commands.UserRegister;

public record RegisterUserCommand : IRequest<Result>
{
    public required string Email { get; init; }

    public required string Password { get; init; }

    public required string FullName { get; set; }

    public string? IDNo { get; set; }

    public required string Title { get; set; }

    public int Department { get; set; }

    public int Office { get; set; }
}
