using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Features.Role.Commands.RemoveMemberFromRole;

public record RemoveMemberFromRoleCommand : IRequest<Result>
{
    public string UserId { get; set; } = string.Empty;
    public string RoleId { get; set; } = string.Empty;

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<RemoveMemberFromRoleDto, RemoveMemberFromRoleCommand>();
        }
    }
}
