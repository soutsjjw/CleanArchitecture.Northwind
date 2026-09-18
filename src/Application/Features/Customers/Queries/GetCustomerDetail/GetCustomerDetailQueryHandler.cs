using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Features.Customers.Queries.GetCustomerDetail;

public sealed class GetCustomerDetailQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetCustomerDetailQuery, Result<CustomerDetailDto>>
{
    public async Task<Result<CustomerDetailDto>> Handle(
        GetCustomerDetailQuery request,
        CancellationToken cancellationToken)
    {
        var customer = await context.Customers
            .AsNoTracking()
            .Where(value => value.Id == request.Id && !value.IsDelete)
            .Select(value => new CustomerDetailDto(
                value.Id,
                value.CompanyName,
                value.ContactName,
                value.ContactTitle,
                value.Address,
                value.City,
                value.Region,
                value.PostalCode,
                value.Country,
                value.Phone,
                value.Fax))
            .SingleOrDefaultAsync(cancellationToken);

        return customer is null
            ? Result<CustomerDetailDto>.Failure("找不到客戶。", 404)
            : Result<CustomerDetailDto>.Success(customer);
    }
}
