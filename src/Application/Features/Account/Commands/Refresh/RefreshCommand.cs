using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Features.Account.Commands.Refresh;

public record RefreshCommand : IRequest<Result<RefreshVm>>
{
    public string RefreshToken { get; set; }
}
