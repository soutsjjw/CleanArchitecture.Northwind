using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Features.Role.Commands.AddMemberToRole;

public record AddMemberToRoleCommand : IRequest<Result>
{
    public string RoleId { get; init; } = default!;
    public string UserId { get; init; } = default!;
}
