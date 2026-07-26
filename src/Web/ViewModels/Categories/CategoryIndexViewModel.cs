using CleanArchitecture.Northwind.Application.Common.Interfaces;

namespace CleanArchitecture.Northwind.Web.ViewModels.Categories;

public sealed class CategoryIndexViewModel
{
    public string? Keyword { get; init; }

    public bool? IsActive { get; init; }

    public IPaginatedList Pagination { get; init; } = default!;

    public IReadOnlyList<CategoryListItemViewModel> Items { get; init; } = [];
}

public sealed class CategoryListItemViewModel
{
    public int Id { get; init; }

    public string EditProtectedId { get; init; } = string.Empty;

    public string DeleteProtectedId { get; init; } = string.Empty;

    public string SetActiveProtectedId { get; init; } = string.Empty;

    public string CategoryName { get; init; } = string.Empty;

    public string? Description { get; init; }

    public bool IsActive { get; init; }

    public int ProductCount { get; init; }
}
