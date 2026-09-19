namespace CleanArchitecture.Northwind.Application.Features.Dashboard.Queries.GetOperationsDashboard;

/// <summary>
/// 在營運儀錶板中顯示訂單的出貨狀態
/// </summary>
public enum DashboardShippingStatus
{
    /// <summary>
    /// 已出貨
    /// </summary>
    Shipped,

    /// <summary>
    /// 待出貨
    /// </summary>
    Pending,

    /// <summary>
    /// 逾期未出貨
    /// </summary>
    Overdue
}

/// <summary>
/// 營運儀錶板 DTO
/// </summary>
public sealed class OperationsDashboardDto
{
    /// <summary>
    /// 今日訂單數量
    /// </summary>
    public int TodayOrderCount { get; init; }

    /// <summary>
    /// 今日營收
    /// </summary>
    public decimal TodayRevenue { get; init; }

    /// <summary>
    /// 本月訂單數量
    /// </summary>
    public int MonthlyOrderCount { get; init; }

    /// <summary>
    /// 本月營收
    /// </summary>
    public decimal MonthlyRevenue { get; init; }

    /// <summary>
    /// 出貨狀態統計
    /// </summary>
    public IReadOnlyList<ShippingStatusDto> ShippingStatuses { get; init; } = Array.Empty<ShippingStatusDto>();

    /// <summary>
    /// 庫存不足的產品
    /// </summary>
    public IReadOnlyList<LowStockProductDto> LowStockProducts { get; init; } = Array.Empty<LowStockProductDto>();

    /// <summary>
    /// 前五大客戶
    /// </summary>
    public IReadOnlyList<TopCustomerDto> TopCustomers { get; init; } = Array.Empty<TopCustomerDto>();
}

/// <summary>
/// 出貨狀態 DTO
/// </summary>
/// <param name="Status"></param>
/// <param name="Count"></param>
public sealed record ShippingStatusDto(DashboardShippingStatus Status, int Count);

/// <summary>
/// 庫存不足的產品 DTO
/// </summary>
/// <param name="ProductName">產品名稱</param>
/// <param name="UnitsInStock">現有庫存</param>
/// <param name="ReorderLevel">訂購點</param>
public sealed record LowStockProductDto(string ProductName, short UnitsInStock, short ReorderLevel);

/// <summary>
/// 前五大客戶 DTO
/// </summary>
/// <param name="CompanyName">公司名稱</param>
/// <param name="OrderCount">訂單數量</param>
/// <param name="Revenue">營收</param>
public sealed record TopCustomerDto(string CompanyName, int OrderCount, decimal Revenue);
