using CleanArchitecture.Northwind.Domain.Enums;

namespace CleanArchitecture.Northwind.Application.Features.Role.Queries.EditRolePrepare;

public class EditRolePrepareDto
{
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string RoleId { get; set; } = default!;
    public string RoleName { get; set; } = default!;
    public List<EditRoleItemPrepareDto> Items { get; set; } = new();
}

public class EditRoleItemPrepareDto
{
    public string Module { get; set; } = default!;
    public DataScope Scope { get; set; }           // Self/Office/Department/All

    public bool Create { get; set; }               // 勾到此範圍
    public bool Read { get; set; }
    public bool Update { get; set; }
    public bool Delete { get; set; }
    public bool System { get; set; }
}
