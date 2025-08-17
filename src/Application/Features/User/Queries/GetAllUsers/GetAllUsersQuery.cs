using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Features.User.Queries.GetAllUsers;

public record GetAllUsersQuery : IRequest<Result<UsersDto>>
{
    public int PageNumber { get; init; } = 1;

    public int PageSize { get; init; } = 10;

    public string? Email { get; init; }

    public string? FullName { get; init; }

    public int? DepartmentId { get; init; }

    public int? OfficeId { get; init; }
}
