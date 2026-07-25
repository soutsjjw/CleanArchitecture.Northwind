using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Features.Products.Queries.GetProductOptions;

public sealed class GetProductOptionsQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetProductOptionsQuery, Result<IReadOnlyList<ProductOptionDto>>>
{
    public async Task<Result<IReadOnlyList<ProductOptionDto>>> Handle(
        GetProductOptionsQuery request,
        CancellationToken cancellationToken)
    {
        var options = await context.Products
            .AsNoTracking()
            .Where(product => !product.IsDelete && !product.Discontinued)
            .OrderBy(product => product.ProductName)
            .ThenBy(product => product.Id)
            .Select(product => new ProductOptionDto(product.Id, product.ProductName))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<ProductOptionDto>>.Success(options);
    }
}
