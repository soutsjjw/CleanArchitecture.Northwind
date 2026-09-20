using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Features.Suppliers.Queries.GetSuppliers;

public sealed record GetSuppliersQuery : IRequest<Result<PaginatedList<SupplierListItemDto>>>
{
    public string? Keyword { get; init; }

    public bool? IsActive { get; init; }

    public int PageNumber { get; init; } = 1;

    public int PageSize { get; init; } = 10;
}

public sealed record SupplierListItemDto(
    int Id,
    string CompanyName,
    string? ContactName,
    string? Phone,
    string? Country,
    bool IsActive,
    int ProductCount);

public sealed class GetSuppliersQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetSuppliersQuery, Result<PaginatedList<SupplierListItemDto>>>
{
    public async Task<Result<PaginatedList<SupplierListItemDto>>> Handle(
        GetSuppliersQuery request,
        CancellationToken cancellationToken)
    {
        var suppliers = context.Suppliers
            .AsNoTracking()
            .Where(supplier => !supplier.IsDelete);

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var keyword = request.Keyword.Trim();
            suppliers = suppliers.Where(supplier =>
                supplier.CompanyName.Contains(keyword)
                || (supplier.ContactName != null
                    && supplier.ContactName.Contains(keyword))
                || (supplier.Phone != null && supplier.Phone.Contains(keyword))
                || (supplier.Country != null && supplier.Country.Contains(keyword)));
        }

        if (request.IsActive.HasValue)
        {
            suppliers = suppliers.Where(supplier =>
                supplier.IsActive == request.IsActive.Value);
        }

        var query = suppliers
            .OrderBy(supplier => supplier.CompanyName)
            .ThenBy(supplier => supplier.Id)
            .Select(supplier => new SupplierListItemDto(
                supplier.Id,
                supplier.CompanyName,
                supplier.ContactName,
                supplier.Phone,
                supplier.Country,
                supplier.IsActive,
                context.Products.Count(product =>
                    product.SupplierId == supplier.Id && !product.IsDelete)));

        var page = await PaginatedList<SupplierListItemDto>.CreateAsync(
            query,
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        return Result<PaginatedList<SupplierListItemDto>>.Success(page);
    }
}
