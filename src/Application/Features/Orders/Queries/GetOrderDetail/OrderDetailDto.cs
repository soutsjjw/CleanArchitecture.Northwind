namespace CleanArchitecture.Northwind.Application.Features.Orders.Queries.GetOrderDetail;

public class OrderDetailDto
{
    public int Id { get; init; }
    public string? CustomerId { get; init; }
    public string CustomerName { get; init; } = string.Empty;
    public string EmployeeName { get; init; } = string.Empty;
    public DateTime? OrderDate { get; init; }
    public DateTime? RequiredDate { get; init; }
    public DateTime? ShippedDate { get; init; }
    public string ShipperName { get; init; } = string.Empty;
    public decimal Freight { get; init; }
    public string ShipName { get; init; } = string.Empty;
    public string ShipAddress { get; init; } = string.Empty;
    public string ShipCity { get; init; } = string.Empty;
    public string ShipRegion { get; init; } = string.Empty;
    public string ShipPostalCode { get; init; } = string.Empty;
    public string ShipCountry { get; init; } = string.Empty;
    public IReadOnlyList<OrderLineItemDto> Items { get; init; } = Array.Empty<OrderLineItemDto>();
}

public class OrderLineItemDto
{
    public string ProductName { get; init; } = string.Empty;
    public decimal UnitPrice { get; init; }
    public short Quantity { get; init; }
    public float Discount { get; init; }
    public decimal TotalAmount { get; init; }
}
