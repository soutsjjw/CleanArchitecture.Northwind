using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Features.Orders.Queries.GetOrders;

public class GetOrdersQueryHandler : IRequestHandler<GetOrdersQuery, Result<OrdersDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTimeService _dateTimeService;

    public GetOrdersQueryHandler(IApplicationDbContext context, IDateTimeService dateTimeService)
    {
        _context = context;
        _dateTimeService = dateTimeService;
    }

    public async Task<Result<OrdersDto>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        var today = _dateTimeService.Now.Date;
        var ordersQuery = _context.Orders
            .AsNoTracking()
            .Where(order => !order.IsDelete);

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var keyword = request.Keyword.Trim().ToLower();
            ordersQuery = ordersQuery.Where(order =>
                order.Id.ToString().Contains(keyword) ||
                (order.CustomerId != null && order.CustomerId.ToLower().Contains(keyword)) ||
                (order.Customer != null && order.Customer.CompanyName.ToLower().Contains(keyword)) ||
                (order.Employee != null && (order.Employee.FirstName + " " + order.Employee.LastName).ToLower().Contains(keyword)) ||
                (order.Shipper != null && order.Shipper.CompanyName.ToLower().Contains(keyword)) ||
                (order.ShipCity != null && order.ShipCity.ToLower().Contains(keyword)) ||
                (order.ShipCountry != null && order.ShipCountry.ToLower().Contains(keyword)) ||
                order.OrderDetails.Any(detail =>
                    detail.ProductId.ToString().Contains(keyword) ||
                    (detail.Product != null && detail.Product.ProductName.ToLower().Contains(keyword))));
        }

        if (request.ShipperId.HasValue && !request.UnassignedShipper)
        {
            ordersQuery = ordersQuery.Where(order => order.ShipVia == request.ShipperId.Value);
        }

        if (request.UnassignedShipper)
        {
            ordersQuery = ordersQuery.Where(order => !order.ShipVia.HasValue);
        }

        if (!string.IsNullOrWhiteSpace(request.Destination))
        {
            var destination = request.Destination.Trim().ToLower();
            ordersQuery = ordersQuery.Where(order =>
                (order.ShipCity != null && order.ShipCity.ToLower().Contains(destination)) ||
                (order.ShipRegion != null && order.ShipRegion.ToLower().Contains(destination)) ||
                (order.ShipCountry != null && order.ShipCountry.ToLower().Contains(destination)));
        }

        var query = ordersQuery
            .Select(order => new OrderItemDto
            {
                Id = order.Id,
                CustomerId = order.CustomerId,
                CustomerName = order.Customer != null ? order.Customer.CompanyName : string.Empty,
                EmployeeName = order.Employee != null ? order.Employee.FirstName + " " + order.Employee.LastName : string.Empty,
                OrderDate = order.OrderDate,
                RequiredDate = order.RequiredDate,
                ShippedDate = order.ShippedDate,
                ShipperName = order.Shipper != null ? order.Shipper.CompanyName : string.Empty,
                Freight = order.Freight ?? 0m,
                ShipCity = order.ShipCity ?? string.Empty,
                ShipCountry = order.ShipCountry ?? string.Empty,
                LineCount = order.OrderDetails.Count,
                TotalAmount = order.OrderDetails
                    .Sum(detail => detail.UnitPrice * detail.Quantity * (decimal)(1 - detail.Discount)),
                ShippingStatus = order.ShippedDate.HasValue
                    ? OrderShippingStatus.Shipped
                    : order.RequiredDate.HasValue && order.RequiredDate.Value.Date < today
                        ? OrderShippingStatus.Overdue
                        : OrderShippingStatus.Unshipped
            });

        if (request.OrderedFrom.HasValue)
        {
            var orderedFrom = request.OrderedFrom.Value.Date;
            query = query.Where(order => order.OrderDate.HasValue && order.OrderDate.Value.Date >= orderedFrom);
        }

        if (request.OrderedTo.HasValue)
        {
            var orderedTo = request.OrderedTo.Value.Date;
            query = query.Where(order => order.OrderDate.HasValue && order.OrderDate.Value.Date <= orderedTo);
        }

        var shippingOverview = new ShippingOverviewDto(
            await query.CountAsync(order => order.ShippingStatus == OrderShippingStatus.Unshipped, cancellationToken),
            await query.CountAsync(order => order.ShippingStatus == OrderShippingStatus.Shipped, cancellationToken),
            await query.CountAsync(order => order.ShippingStatus == OrderShippingStatus.Overdue, cancellationToken));

        if (request.ShippingStatus.HasValue && request.ShippingStatus.Value != OrderShippingStatus.All)
        {
            query = query.Where(order => order.ShippingStatus == request.ShippingStatus.Value);
        }

        query = (request.SortBy, request.SortDescending) switch
        {
            (OrderSortField.Id, false) => query.OrderBy(order => order.Id),
            (OrderSortField.Id, true) => query.OrderByDescending(order => order.Id),
            (OrderSortField.CustomerName, false) => query.OrderBy(order => order.CustomerName).ThenBy(order => order.Id),
            (OrderSortField.CustomerName, true) => query.OrderByDescending(order => order.CustomerName).ThenByDescending(order => order.Id),
            (OrderSortField.OrderDate, false) => query.OrderBy(order => order.OrderDate).ThenBy(order => order.Id),
            (OrderSortField.OrderDate, true) => query.OrderByDescending(order => order.OrderDate).ThenByDescending(order => order.Id),
            (OrderSortField.ShippingStatus, false) => query.OrderBy(order => order.ShippingStatus).ThenBy(order => order.Id),
            (OrderSortField.ShippingStatus, true) => query.OrderByDescending(order => order.ShippingStatus).ThenByDescending(order => order.Id),
            (OrderSortField.TotalAmount, false) => query.OrderBy(order => order.TotalAmount).ThenBy(order => order.Id),
            (OrderSortField.TotalAmount, true) => query.OrderByDescending(order => order.TotalAmount).ThenByDescending(order => order.Id),
            _ => query.OrderByDescending(order => order.OrderDate).ThenByDescending(order => order.Id)
        };

        var orders = await PaginatedList<OrderItemDto>.CreateAsync(
            query,
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        var shipperOptions = await _context.Shippers
            .AsNoTracking()
            .OrderBy(shipper => shipper.CompanyName)
            .Select(shipper => new ShipperOptionDto(shipper.Id, shipper.CompanyName))
            .ToListAsync(cancellationToken);

        return await Result<OrdersDto>.SuccessAsync(new OrdersDto
        {
            Keyword = request.Keyword,
            OrderedFrom = request.OrderedFrom,
            OrderedTo = request.OrderedTo,
            ShippingStatus = request.ShippingStatus,
            ShipperId = request.ShipperId,
            UnassignedShipper = request.UnassignedShipper,
            Destination = request.Destination,
            SortBy = request.SortBy,
            SortDescending = request.SortDescending,
            Orders = orders,
            ShippingOverview = shippingOverview,
            ShipperOptions = shipperOptions
        });
    }
}
