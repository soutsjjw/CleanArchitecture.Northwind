using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Features.User.Queries.GetAllUsers;

public record GetAllUsersQuery : IRequest<Result<PaginatedList<UsersDto>>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}
