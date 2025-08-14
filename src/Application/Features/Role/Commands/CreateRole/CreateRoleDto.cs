namespace CleanArchitecture.Northwind.Application.Features.Role.Commands.CreateRole;

public record CreateRoleDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; init; } = string.Empty;
}
