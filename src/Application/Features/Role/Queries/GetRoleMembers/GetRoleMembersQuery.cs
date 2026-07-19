using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Features.Role.Queries.GetRoleMembers;

public record GetRoleMembersQuery : IRequest<Result<List<MembersDto>>>
{
    public string RoleId { get; init; } = default!;
}
