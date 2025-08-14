namespace CleanArchitecture.Northwind.Application.Features.Role.Queries.GetAllRoles;

public class RolesDto
{
    public string Id { get; set; } = default!;

    public string Name { get; set; } = default!;

    public string Description { get; set; } = default!;

    public int Sort { get; set; } = int.MaxValue;
}
