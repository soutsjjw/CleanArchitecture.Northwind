using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Features.Account.Commands.UserAPILogin;

public record UserAPILoginCommand : IRequest<Result<UserAPILoginVm>>
{
    public string UserName { get; set; } = "";

    public string Password { get; set; } = "";
}
