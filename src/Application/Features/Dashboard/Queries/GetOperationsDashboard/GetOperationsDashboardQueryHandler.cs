using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Features.Dashboard.Queries.GetOperationsDashboard;

/// <summary>
/// 取得營運儀錶板查詢處理程式
/// </summary>
/// <param name="context"></param>
/// <param name="dateTimeService"></param>
public sealed class GetOperationsDashboardQueryHandler(
    IApplicationDbContext context,
    IDateTimeService dateTimeService) : IRequestHandler<GetOperationsDashboardQuery, Result<OperationsDashboardDto>>
{
    /// <summary>
    /// 處理取得營運儀錶板查詢
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Result<OperationsDashboardDto>> Handle(GetOperationsDashboardQuery request, CancellationToken cancellationToken)
    {
        var today = dateTimeService.Now.Date;
        var tomorrow = today.AddDays(1);
        var monthStart = new DateTime(today.Year, today.Month, 1);
        var nextMonthStart = monthStart.AddMonths(1);

        var monthlyOrders = await context.Orders
            .AsNoTracking()
            .Where(order => !order.IsDelete
                && order.OrderDate.HasValue
                && order.OrderDate.Value >= monthStart
                && order.OrderDate.Value < nextMonthStart)
            .Select(order => new MonthlyOrderRow(
                order.Id,
                order.Customer != null ? order.Customer.CompanyName : string.Empty,
                order.OrderDate!.Value,
                order.ShippedDate,
                order.RequiredDate,
                order.OrderDetails.Sum(detail => detail.UnitPrice * detail.Quantity * (decimal)(1 - detail.Discount))))
            .ToListAsync(cancellationToken);

        var lowStockProducts = await context.Products
            .AsNoTracking()
            .Where(product => !product.IsDelete
                && !product.Discontinued
                && product.UnitsInStock <= product.ReorderLevel)
            .OrderByDescending(product => (product.ReorderLevel ?? 0) - (product.UnitsInStock ?? 0))
            .ThenBy(product => product.ProductName)
            .Take(5)
            .Select(product => new LowStockProductDto(
                product.ProductName,
                product.UnitsInStock ?? 0,
                product.ReorderLevel ?? 0))
            .ToListAsync(cancellationToken);

        var shippingStatuses = new[]
        {
            new ShippingStatusDto(DashboardShippingStatus.Shipped, monthlyOrders.Count(order => order.ShippedDate.HasValue)),
            new ShippingStatusDto(DashboardShippingStatus.Pending, monthlyOrders.Count(order => !order.ShippedDate.HasValue && (!order.RequiredDate.HasValue || order.RequiredDate.Value.Date >= today))),
            new ShippingStatusDto(DashboardShippingStatus.Overdue, monthlyOrders.Count(order => !order.ShippedDate.HasValue && order.RequiredDate.HasValue && order.RequiredDate.Value.Date < today))
        };

        var topCustomers = monthlyOrders
            .GroupBy(order => order.CustomerName)
            .Select(group => new TopCustomerDto(group.Key, group.Count(), group.Sum(order => order.Revenue)))
            .OrderByDescending(customer => customer.Revenue)
            .ThenByDescending(customer => customer.OrderCount)
            .ThenBy(customer => customer.CompanyName)
            .Take(5)
            .ToList();

        var todayOrders = monthlyOrders.Where(order => order.OrderDate >= today && order.OrderDate < tomorrow).ToList();

        return await Result<OperationsDashboardDto>.SuccessAsync(new OperationsDashboardDto
        {
            TodayOrderCount = todayOrders.Count,
            TodayRevenue = todayOrders.Sum(order => order.Revenue),
            MonthlyOrderCount = monthlyOrders.Count,
            MonthlyRevenue = monthlyOrders.Sum(order => order.Revenue),
            ShippingStatuses = shippingStatuses,
            LowStockProducts = lowStockProducts,
            TopCustomers = topCustomers
        });
    }

    /// <summary>
    /// 每月訂單資料列
    /// </summary>
    /// <param name="Id">訂單ID</param>
    /// <param name="CustomerName">客戶名稱</param>
    /// <param name="OrderDate">訂單日期</param>
    /// <param name="ShippedDate">出貨日期</param>
    /// <param name="RequiredDate">需求日期</param>
    /// <param name="Revenue">營收</param>
    private sealed record MonthlyOrderRow(
        int Id,
        string CustomerName,
        DateTime OrderDate,
        DateTime? ShippedDate,
        DateTime? RequiredDate,
        decimal Revenue);
}
