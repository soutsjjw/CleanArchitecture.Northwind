namespace CleanArchitecture.Northwind.Application.Features.User.Queries.GetAllUsers;

public class UsersDto
{
    public string UserName { get; init; } = default!;
    public string? Email { get; init; }
    public string? FullName { get; init; }
    public string? Title { get; init; }
}
