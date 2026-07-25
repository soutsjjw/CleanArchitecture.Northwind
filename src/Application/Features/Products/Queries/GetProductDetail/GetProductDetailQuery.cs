using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Features.Products.Queries.GetProductDetail;

public sealed record GetProductDetailQuery(int Id)
    : IRequest<Result<ProductDetailDto>>;

public sealed record ProductDetailDto(
    int Id,
    string ProductName,
    int CategoryId,
    string CategoryName,
    int SupplierId,
    string SupplierName,
    string? QuantityPerUnit,
    decimal UnitPrice,
    short UnitsInStock,
    short UnitsOnOrder,
    short ReorderLevel,
    bool Discontinued,
    byte[] RowVersion,
    byte[]? Picture,
    string? PictureContentType);

public sealed class GetProductDetailQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetProductDetailQuery, Result<ProductDetailDto>>
{
    public async Task<Result<ProductDetailDto>> Handle(
        GetProductDetailQuery request,
        CancellationToken cancellationToken)
    {
        var product = await context.Products
            .AsNoTracking()
            .Where(value => value.Id == request.Id && !value.IsDelete)
            .Select(value => new ProductDetailDto(
                value.Id,
                value.ProductName,
                value.CategoryId ?? 0,
                value.Category != null ? value.Category.CategoryName : string.Empty,
                value.SupplierId ?? 0,
                value.Supplier != null ? value.Supplier.CompanyName : string.Empty,
                value.QuantityPerUnit,
                value.UnitPrice ?? 0,
                value.UnitsInStock ?? 0,
                value.UnitsOnOrder ?? 0,
                value.ReorderLevel ?? 0,
                value.Discontinued,
                value.RowVersion,
                value.Picture,
                value.PictureContentType))
            .SingleOrDefaultAsync(cancellationToken);

        return product is null
            ? Result<ProductDetailDto>.Failure("找不到商品。", 404)
            : Result<ProductDetailDto>.Success(product);
    }
}
