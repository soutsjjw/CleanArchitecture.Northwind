using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Common.Models;
using CleanArchitecture.Northwind.Application.Features.Orders.Queries.GetOrders;

namespace CleanArchitecture.Northwind.Application.Features.Customers.Queries.GetCustomerOrderHistory;

public sealed class GetCustomerOrderHistoryQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetCustomerOrderHistoryQuery, Result<CustomerOrderHistoryDto>>
{
    public async Task<Result<CustomerOrderHistoryDto>> Handle(
        GetCustomerOrderHistoryQuery request,
        CancellationToken cancellationToken)
    {
        var today = DateTime.Today;
        var orders = context.Orders
            .AsNoTracking()
            .Where(order => !order.IsDelete && order.CustomerId == request.CustomerId)
            .OrderByDescending(order => order.OrderDate)
            .ThenByDescending(order => order.Id)
            .Select(order => new CustomerOrderHistoryItemDto
            {
                Id = order.Id,
                OrderDate = order.OrderDate,
                TotalAmount = order.OrderDetails
                    .Sum(detail => detail.UnitPrice * detail.Quantity * (decimal)(1 - detail.Discount)),
                ShippingStatus = order.ShippedDate.HasValue
                    ? OrderShippingStatus.Shipped
                    : order.RequiredDate.HasValue && order.RequiredDate.Value.Date < today
                        ? OrderShippingStatus.Overdue
                        : OrderShippingStatus.Unshipped,
                ShipperName = order.Shipper != null ? order.Shipper.CompanyName : string.Empty
            });

        var page = await PaginatedList<CustomerOrderHistoryItemDto>.CreateAsync(
            orders,
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        return Result<CustomerOrderHistoryDto>.Success(new CustomerOrderHistoryDto
        {
            Orders = page
        });
    }
}
