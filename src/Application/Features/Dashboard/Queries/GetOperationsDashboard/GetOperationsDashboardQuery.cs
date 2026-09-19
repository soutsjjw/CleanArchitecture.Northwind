using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Features.Dashboard.Queries.GetOperationsDashboard;

/// <summary>
/// 取得營運儀錶板查詢
/// </summary>
public record GetOperationsDashboardQuery : IRequest<Result<OperationsDashboardDto>>;
