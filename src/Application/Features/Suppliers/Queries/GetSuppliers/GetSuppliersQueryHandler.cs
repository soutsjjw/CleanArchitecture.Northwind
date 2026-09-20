using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Features.Suppliers.Queries.GetSuppliers;

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

        suppliers = (request.SortBy, request.SortDescending) switch
        {
            (SupplierSortField.CompanyName, true) => suppliers.OrderByDescending(supplier => supplier.CompanyName).ThenByDescending(supplier => supplier.Id),
            (SupplierSortField.ContactName, false) => suppliers.OrderBy(supplier => supplier.ContactName).ThenBy(supplier => supplier.Id),
            (SupplierSortField.ContactName, true) => suppliers.OrderByDescending(supplier => supplier.ContactName).ThenByDescending(supplier => supplier.Id),
            (SupplierSortField.Phone, false) => suppliers.OrderBy(supplier => supplier.Phone).ThenBy(supplier => supplier.Id),
            (SupplierSortField.Phone, true) => suppliers.OrderByDescending(supplier => supplier.Phone).ThenByDescending(supplier => supplier.Id),
            (SupplierSortField.Country, false) => suppliers.OrderBy(supplier => supplier.Country).ThenBy(supplier => supplier.Id),
            (SupplierSortField.Country, true) => suppliers.OrderByDescending(supplier => supplier.Country).ThenByDescending(supplier => supplier.Id),
            (SupplierSortField.IsActive, false) => suppliers.OrderBy(supplier => supplier.IsActive).ThenBy(supplier => supplier.Id),
            (SupplierSortField.IsActive, true) => suppliers.OrderByDescending(supplier => supplier.IsActive).ThenByDescending(supplier => supplier.Id),
            (SupplierSortField.ProductCount, false) => suppliers.OrderBy(supplier => context.Products.Count(product => product.SupplierId == supplier.Id && !product.IsDelete)).ThenBy(supplier => supplier.Id),
            (SupplierSortField.ProductCount, true) => suppliers.OrderByDescending(supplier => context.Products.Count(product => product.SupplierId == supplier.Id && !product.IsDelete)).ThenByDescending(supplier => supplier.Id),
            _ => suppliers.OrderBy(supplier => supplier.CompanyName).ThenBy(supplier => supplier.Id)
        };

        var query = suppliers
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
