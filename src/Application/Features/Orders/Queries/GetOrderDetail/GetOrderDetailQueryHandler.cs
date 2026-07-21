using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Northwind.Application.Features.Orders.Queries.GetOrderDetail;

public class GetOrderDetailQueryHandler(IApplicationDbContext context) : IRequestHandler<GetOrderDetailQuery, Result<OrderDetailDto>>
{
    public async Task<Result<OrderDetailDto>> Handle(GetOrderDetailQuery request, CancellationToken cancellationToken)
    {
        var order = await context.Orders
            .AsNoTracking()
            .Where(x => x.Id == request.Id && !x.IsDelete)
            .Select(x => new OrderDetailDto
            {
                Id = x.Id,
                CustomerId = x.CustomerId,
                CustomerName = x.Customer != null ? x.Customer.CompanyName : string.Empty,
                EmployeeName = x.Employee != null ? x.Employee.FirstName + " " + x.Employee.LastName : string.Empty,
                OrderDate = x.OrderDate,
                RequiredDate = x.RequiredDate,
                ShippedDate = x.ShippedDate,
                ShipperName = x.Shipper != null ? x.Shipper.CompanyName : string.Empty,
                Freight = x.Freight ?? 0m,
                ShipName = x.ShipName ?? string.Empty,
                ShipAddress = x.ShipAddress ?? string.Empty,
                ShipCity = x.ShipCity ?? string.Empty,
                ShipRegion = x.ShipRegion ?? string.Empty,
                ShipPostalCode = x.ShipPostalCode ?? string.Empty,
                ShipCountry = x.ShipCountry ?? string.Empty,
                Items = x.OrderDetails.Select(detail => new OrderLineItemDto
                {
                    ProductName = detail.Product.ProductName,
                    UnitPrice = detail.UnitPrice,
                    Quantity = detail.Quantity,
                    Discount = detail.Discount,
                    TotalAmount = detail.UnitPrice * detail.Quantity * (decimal)(1 - detail.Discount)
                }).ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        return order is null
            ? await Result<OrderDetailDto>.FailureAsync("找不到訂單。")
            : await Result<OrderDetailDto>.SuccessAsync(order);
    }
}
