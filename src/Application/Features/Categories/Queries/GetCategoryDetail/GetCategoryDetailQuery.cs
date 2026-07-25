using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Features.Categories.Queries.GetCategoryDetail;

public sealed record GetCategoryDetailQuery(int Id)
    : IRequest<Result<CategoryDetailDto>>;

public sealed record CategoryDetailDto(
    int Id,
    string CategoryName,
    string? Description,
    bool IsActive,
    int ProductCount);

public sealed class GetCategoryDetailQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetCategoryDetailQuery, Result<CategoryDetailDto>>
{
    public async Task<Result<CategoryDetailDto>> Handle(
        GetCategoryDetailQuery request,
        CancellationToken cancellationToken)
    {
        var category = await context.Categories
            .AsNoTracking()
            .Where(value => value.Id == request.Id && !value.IsDelete)
            .Select(value => new CategoryDetailDto(
                value.Id,
                value.CategoryName,
                value.Description,
                value.IsActive,
                value.Products.Count(product => !product.IsDelete)))
            .SingleOrDefaultAsync(cancellationToken);

        return category is null
            ? Result<CategoryDetailDto>.Failure("找不到分類。", 404)
            : Result<CategoryDetailDto>.Success(category);
    }
}
