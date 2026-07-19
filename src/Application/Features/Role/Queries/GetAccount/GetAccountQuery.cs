using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Features.Role.Queries.GetAccount;

public record GetAccountQuery : IRequest<Result<List<AccountVm>>>
{
    public int? DepartmentId { get; init; }

    public int? OfficeId { get; init; }
}
