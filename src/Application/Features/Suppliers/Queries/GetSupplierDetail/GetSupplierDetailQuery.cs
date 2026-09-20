using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Features.Suppliers.Queries.GetSupplierDetail;

public sealed record GetSupplierDetailQuery(int Id) : IRequest<Result<SupplierDetailDto>>;
