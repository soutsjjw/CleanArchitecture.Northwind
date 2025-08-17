using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Features.User.Queries.UserDetail;

public record UserDetailQuery : IRequest<Result<UserDetailDto>>
{
    public string UserId { get; set; } = default!;
}
