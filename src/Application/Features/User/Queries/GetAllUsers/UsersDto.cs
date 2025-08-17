using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Features.User.Queries.GetAllUsers;

public class UsersDto
{
    public string? Email { get; init; }

    public string? FullName { get; init; }

    public int? DepartmentId { get; init; }

    public int? OfficeId { get; init; }

    public PaginatedList<UserItems> Users { get; init; } = default!;
}

public class UserItems
{
    public string UserId { get; set; } = default!;
    public string UserName { get; init; } = default!;
    public string? Email { get; init; }
    public string? FullName { get; init; }
    public string? Title { get; init; }
    public int? DepartmentId { get; init; }
    public int? OfficeId { get; init; }
}
