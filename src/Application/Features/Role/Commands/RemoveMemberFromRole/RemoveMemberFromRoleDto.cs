namespace CleanArchitecture.Northwind.Application.Features.Role.Commands.RemoveMemberFromRole;

public class RemoveMemberFromRoleDto
{
    public string UserId { get; init; } = string.Empty;
    public string RoleId { get; init; } = string.Empty;
}
