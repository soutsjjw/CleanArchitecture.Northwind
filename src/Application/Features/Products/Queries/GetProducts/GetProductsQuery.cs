using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Features.Products.Queries.GetProducts;

public sealed record GetProductsQuery
    : IRequest<Result<PaginatedList<ProductListItemDto>>>
{
    public string? Keyword { get; init; }

    public int? CategoryId { get; init; }

    public int? SupplierId { get; init; }

    public bool? Discontinued { get; init; }

    public bool LowStockOnly { get; init; }

    public int PageNumber { get; init; } = 1;

    public int PageSize { get; init; } = 10;
}

public sealed record ProductListItemDto(
    int Id,
    string ProductName,
    int CategoryId,
    string CategoryName,
    int SupplierId,
    string SupplierName,
    decimal UnitPrice,
    short UnitsInStock,
    short ReorderLevel,
    bool Discontinued,
    bool HasPicture,
    byte[] RowVersion);

public sealed class GetProductsQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetProductsQuery, Result<PaginatedList<ProductListItemDto>>>
{
    public async Task<Result<PaginatedList<ProductListItemDto>>> Handle(
        GetProductsQuery request,
        CancellationToken cancellationToken)
    {
        var products = context.Products
            .AsNoTracking()
            .Where(product => !product.IsDelete);

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var keyword = request.Keyword.Trim();
            products = products.Where(product =>
                product.ProductName.Contains(keyword)
                || (product.Category != null
                    && product.Category.CategoryName.Contains(keyword))
                || (product.Supplier != null
                    && product.Supplier.CompanyName.Contains(keyword)));
        }

        if (request.CategoryId.HasValue)
        {
            products = products.Where(product =>
                product.CategoryId == request.CategoryId.Value);
        }

        if (request.SupplierId.HasValue)
        {
            products = products.Where(product =>
                product.SupplierId == request.SupplierId.Value);
        }

        if (request.Discontinued.HasValue)
        {
            products = products.Where(product =>
                product.Discontinued == request.Discontinued.Value);
        }

        if (request.LowStockOnly)
        {
            products = products.Where(product =>
                !product.Discontinued
                && (product.UnitsInStock ?? 0) <= (product.ReorderLevel ?? 0));
        }

        var query = products
            .OrderBy(product => product.ProductName)
            .ThenBy(product => product.Id)
            .Select(product => new ProductListItemDto(
                product.Id,
                product.ProductName,
                product.CategoryId ?? 0,
                product.Category != null ? product.Category.CategoryName : string.Empty,
                product.SupplierId ?? 0,
                product.Supplier != null ? product.Supplier.CompanyName : string.Empty,
                product.UnitPrice ?? 0,
                product.UnitsInStock ?? 0,
                product.ReorderLevel ?? 0,
                product.Discontinued,
                product.Picture != null,
                product.RowVersion));

        var result = await PaginatedList<ProductListItemDto>.CreateAsync(
            query,
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        return Result<PaginatedList<ProductListItemDto>>.Success(result);
    }
}
