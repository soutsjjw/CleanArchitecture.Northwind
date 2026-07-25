using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Features.Products.Queries.GetProductFormOptions;

public sealed record GetProductFormOptionsQuery
    : IRequest<Result<ProductFormOptionsDto>>;

public sealed record ProductFormOptionDto(int Id, string Name);

public sealed record ProductFormOptionsDto(
    IReadOnlyList<ProductFormOptionDto> Categories,
    IReadOnlyList<ProductFormOptionDto> Suppliers);

public sealed class GetProductFormOptionsQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetProductFormOptionsQuery, Result<ProductFormOptionsDto>>
{
    public async Task<Result<ProductFormOptionsDto>> Handle(
        GetProductFormOptionsQuery request,
        CancellationToken cancellationToken)
    {
        var categories = await context.Categories
            .AsNoTracking()
            .Where(category => !category.IsDelete && category.IsActive)
            .OrderBy(category => category.CategoryName)
            .ThenBy(category => category.Id)
            .Select(category =>
                new ProductFormOptionDto(category.Id, category.CategoryName))
            .ToListAsync(cancellationToken);

        var suppliers = await context.Suppliers
            .AsNoTracking()
            .Where(supplier => !supplier.IsDelete && supplier.IsActive)
            .OrderBy(supplier => supplier.CompanyName)
            .ThenBy(supplier => supplier.Id)
            .Select(supplier =>
                new ProductFormOptionDto(supplier.Id, supplier.CompanyName))
            .ToListAsync(cancellationToken);

        return Result<ProductFormOptionsDto>.Success(
            new ProductFormOptionsDto(categories, suppliers));
    }
}
