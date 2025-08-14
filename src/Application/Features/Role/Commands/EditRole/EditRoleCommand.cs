using CleanArchitecture.Northwind.Application.Common.Models;
using CleanArchitecture.Northwind.Application.Features.Role.Queries.EditRolePrepare;
using CleanArchitecture.Northwind.Domain.Enums;

namespace CleanArchitecture.Northwind.Application.Features.Role.Commands.EditRole;

public record EditRoleCommand : IRequest<Result>
{
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string RoleId { get; set; } = default!;
    public string RoleName { get; set; } = default!;
    public List<EditRoleItemCommand> Items { get; set; } = new();

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<EditRolePrepareDto, EditRoleCommand>();
        }
    }
}

public class EditRoleItemCommand
{
    public string Module { get; set; } = default!;
    public DataScope Scope { get; set; }           // Self/Office/Department/All

    public bool Create { get; set; }               // 勾到此範圍
    public bool Read { get; set; }
    public bool Update { get; set; }
    public bool Delete { get; set; }
    public bool System { get; set; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<EditRoleItemPrepareDto, EditRoleItemCommand>();
        }
    }
}
