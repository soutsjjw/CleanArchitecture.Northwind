using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Features.Account.Commands.UserLogin;

public record UserLoginCommand : IRequest<Result<UserLoginVm>>
{
    public string UserName { get; set; } = "";

    public string Password { get; set; } = "";
}
