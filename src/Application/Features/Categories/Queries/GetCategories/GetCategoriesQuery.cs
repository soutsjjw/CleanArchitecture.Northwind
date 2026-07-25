using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Features.Categories.Queries.GetCategories;

public sealed record GetCategoriesQuery
    : IRequest<Result<PaginatedList<CategoryListItemDto>>>
{
    public string? Keyword { get; init; }

    public bool? IsActive { get; init; }

    public int PageNumber { get; init; } = 1;

    public int PageSize { get; init; } = 10;
}

public sealed record CategoryListItemDto(
    int Id,
    string CategoryName,
    string? Description,
    bool IsActive,
    int ProductCount);

public sealed class GetCategoriesQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetCategoriesQuery, Result<PaginatedList<CategoryListItemDto>>>
{
    public async Task<Result<PaginatedList<CategoryListItemDto>>> Handle(
        GetCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        var categories = context.Categories
            .AsNoTracking()
            .Where(category => !category.IsDelete);

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var keyword = request.Keyword.Trim();
            categories = categories.Where(category =>
                category.CategoryName.Contains(keyword)
                || (category.Description != null
                    && category.Description.Contains(keyword)));
        }

        if (request.IsActive.HasValue)
        {
            categories = categories.Where(category =>
                category.IsActive == request.IsActive.Value);
        }

        var query = categories
            .OrderBy(category => category.CategoryName)
            .ThenBy(category => category.Id)
            .Select(category => new CategoryListItemDto(
                category.Id,
                category.CategoryName,
                category.Description,
                category.IsActive,
                category.Products.Count(product => !product.IsDelete)));

        var result = await PaginatedList<CategoryListItemDto>.CreateAsync(
            query,
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        return Result<PaginatedList<CategoryListItemDto>>.Success(result);
    }
}
