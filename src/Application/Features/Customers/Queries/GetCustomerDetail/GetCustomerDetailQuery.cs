using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Features.Customers.Queries.GetCustomerDetail;

public sealed record GetCustomerDetailQuery(string Id)
    : IRequest<Result<CustomerDetailDto>>;
