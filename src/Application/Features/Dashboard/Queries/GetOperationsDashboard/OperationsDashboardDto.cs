namespace CleanArchitecture.Northwind.Application.Features.Dashboard.Queries.GetOperationsDashboard;

public enum DashboardShippingStatus
{
    Shipped,
    Pending,
    Overdue
}

public sealed class OperationsDashboardDto
{
    public int TodayOrderCount { get; init; }
    public decimal TodayRevenue { get; init; }
    public int MonthlyOrderCount { get; init; }
    public decimal MonthlyRevenue { get; init; }
    public IReadOnlyList<ShippingStatusDto> ShippingStatuses { get; init; } = Array.Empty<ShippingStatusDto>();
    public IReadOnlyList<LowStockProductDto> LowStockProducts { get; init; } = Array.Empty<LowStockProductDto>();
    public IReadOnlyList<TopCustomerDto> TopCustomers { get; init; } = Array.Empty<TopCustomerDto>();
}

public sealed record ShippingStatusDto(DashboardShippingStatus Status, int Count);
public sealed record LowStockProductDto(string ProductName, short UnitsInStock, short ReorderLevel);
public sealed record TopCustomerDto(string CompanyName, int OrderCount, decimal Revenue);
