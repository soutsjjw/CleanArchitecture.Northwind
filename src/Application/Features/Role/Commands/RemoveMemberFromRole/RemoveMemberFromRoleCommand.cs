using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Features.Role.Commands.RemoveMemberFromRole;

public record RemoveMemberFromRoleCommand : IRequest<Result>
{
    public string UserId { get; init; } = string.Empty;
    public string RoleId { get; init; } = string.Empty;

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<RemoveMemberFromRoleDto, RemoveMemberFromRoleCommand>();
        }
    }
}
