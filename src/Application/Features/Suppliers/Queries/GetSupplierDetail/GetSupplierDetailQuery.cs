using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Features.Suppliers.Queries.GetSupplierDetail;

public sealed record GetSupplierDetailQuery(int Id) : IRequest<Result<SupplierDetailDto>>;

public sealed record SupplierDetailDto(
    int Id,
    string CompanyName,
    string? ContactName,
    string? ContactTitle,
    string? Address,
    string? City,
    string? Region,
    string? PostalCode,
    string? Country,
    string? Phone,
    string? Fax,
    string? HomePage,
    bool IsActive,
    int ProductCount);

public sealed class GetSupplierDetailQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetSupplierDetailQuery, Result<SupplierDetailDto>>
{
    public async Task<Result<SupplierDetailDto>> Handle(
        GetSupplierDetailQuery request,
        CancellationToken cancellationToken)
    {
        var supplier = await context.Suppliers
            .AsNoTracking()
            .Where(value => value.Id == request.Id && !value.IsDelete)
            .Select(value => new SupplierDetailDto(
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
                value.Fax,
                value.HomePage,
                value.IsActive,
                context.Products.Count(product =>
                    product.SupplierId == value.Id && !product.IsDelete)))
            .SingleOrDefaultAsync(cancellationToken);

        return supplier is null
            ? Result<SupplierDetailDto>.Failure("找不到供應商。", 404)
            : Result<SupplierDetailDto>.Success(supplier);
    }
}
