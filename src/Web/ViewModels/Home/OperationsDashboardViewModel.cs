using CleanArchitecture.Northwind.Application.Features.Dashboard.Queries.GetOperationsDashboard;

namespace CleanArchitecture.Northwind.Web.ViewModels.Home;

public sealed class OperationsDashboardViewModel
{
    public int TodayOrderCount { get; init; }
    public decimal TodayRevenue { get; init; }
    public int MonthlyOrderCount { get; init; }
    public decimal MonthlyRevenue { get; init; }
    public IReadOnlyList<ShippingStatusViewModel> ShippingStatuses { get; init; } = Array.Empty<ShippingStatusViewModel>();
    public IReadOnlyList<LowStockProductViewModel> LowStockProducts { get; init; } = Array.Empty<LowStockProductViewModel>();
    public IReadOnlyList<TopCustomerViewModel> TopCustomers { get; init; } = Array.Empty<TopCustomerViewModel>();
}

public sealed record ShippingStatusViewModel(DashboardShippingStatus Status, int Count);
public sealed record LowStockProductViewModel(string ProductName, short UnitsInStock, short ReorderLevel);
public sealed record TopCustomerViewModel(string CompanyName, int OrderCount, decimal Revenue);
