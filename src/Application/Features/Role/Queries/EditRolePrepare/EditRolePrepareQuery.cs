using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Features.Role.Queries.EditRolePrepare;

public record EditRolePrepareQuery : IRequest<Result<EditRolePrepareDto>>
{
    public string RoleId { get; set; } = string.Empty;
}
