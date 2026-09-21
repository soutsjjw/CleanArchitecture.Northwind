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
