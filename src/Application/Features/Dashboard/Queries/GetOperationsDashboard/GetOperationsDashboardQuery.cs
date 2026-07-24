using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Features.Dashboard.Queries.GetOperationsDashboard;

public record GetOperationsDashboardQuery : IRequest<Result<OperationsDashboardDto>>;
