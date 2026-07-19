using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Features.Role.Queries.GetAllRoles;

public record GetAllRolesQuery : IRequest<Result<List<RolesDto>>>
{
}
