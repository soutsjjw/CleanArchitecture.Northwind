using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Features.Products.Queries.GetPublicProducts;

public sealed record GetPublicProductsQuery
    : IRequest<Result<PublicProductCatalogDto>>
{
    public string? Keyword { get; init; }

    public int? CategoryId { get; init; }

    public int PageNumber { get; init; } = 1;

    public int PageSize { get; init; } = 12;
}

public sealed record PublicProductCatalogDto(
    PaginatedList<PublicProductListItemDto> Products,
    IReadOnlyList<PublicProductCategoryDto> Categories);

public sealed record PublicProductListItemDto(
    int Id,
    string ProductName,
    string CategoryName,
    decimal UnitPrice,
    bool HasPicture);

public sealed record PublicProductCategoryDto(int Id, string Name);

public sealed record GetPublicProductImageQuery(int Id)
    : IRequest<Result<PublicProductImageDto>>;

public sealed record PublicProductImageDto(byte[] Picture, string PictureContentType);

public sealed class GetPublicProductsQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetPublicProductsQuery, Result<PublicProductCatalogDto>>
{
    public async Task<Result<PublicProductCatalogDto>> Handle(
        GetPublicProductsQuery request,
        CancellationToken cancellationToken)
    {
        var products = context.Products
            .AsNoTracking()
            .Where(product => !product.IsDelete && !product.Discontinued);

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var keyword = request.Keyword.Trim();
            products = products.Where(product =>
                product.ProductName.Contains(keyword)
                || (product.Category != null
                    && product.Category.CategoryName.Contains(keyword)));
        }

        if (request.CategoryId.HasValue)
        {
            products = products.Where(product =>
                product.CategoryId == request.CategoryId.Value);
        }

        var productPage = await PaginatedList<PublicProductListItemDto>.CreateAsync(
            products
                .OrderBy(product => product.ProductName)
                .ThenBy(product => product.Id)
                .Select(product => new PublicProductListItemDto(
                    product.Id,
                    product.ProductName,
                    product.Category != null ? product.Category.CategoryName : string.Empty,
                    product.UnitPrice ?? 0,
                    product.Picture != null)),
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        var categories = await context.Categories
            .AsNoTracking()
            .Where(category => category.IsActive && !category.IsDelete)
            .OrderBy(category => category.CategoryName)
            .Select(category => new PublicProductCategoryDto(
                category.Id,
                category.CategoryName))
            .ToListAsync(cancellationToken);

        return Result<PublicProductCatalogDto>.Success(
            new PublicProductCatalogDto(productPage, categories));
    }
}

public sealed class GetPublicProductImageQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetPublicProductImageQuery, Result<PublicProductImageDto>>
{
    public async Task<Result<PublicProductImageDto>> Handle(
        GetPublicProductImageQuery request,
        CancellationToken cancellationToken)
    {
        var image = await context.Products
            .AsNoTracking()
            .Where(product => product.Id == request.Id
                && !product.IsDelete
                && !product.Discontinued
                && product.Picture != null
                && product.PictureContentType != null)
            .Select(product => new PublicProductImageDto(
                product.Picture!,
                product.PictureContentType!))
            .SingleOrDefaultAsync(cancellationToken);

        return image is null
            ? Result<PublicProductImageDto>.Failure("找不到商品圖片。", 404)
            : Result<PublicProductImageDto>.Success(image);
    }
}
