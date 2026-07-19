using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Features.User.Commands.ResendConfirmationEmail;

public record ResendConfirmationEmailCommand : IRequest<Result>
{
    public string UserId { get; set; } = string.Empty;
}
