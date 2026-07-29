using CleanArchitecture.Northwind.Application.Common.Interfaces;

namespace CleanArchitecture.Northwind.Web.ViewModels.Home;

public sealed class PublicProductCatalogViewModel
{
    public string? Keyword { get; init; }

    public int? CategoryId { get; init; }

    public IPaginatedList Pagination { get; init; } = default!;

    public IReadOnlyList<PublicProductCardViewModel> Items { get; init; } = [];

    public IReadOnlyList<PublicProductCategoryViewModel> Categories { get; init; } = [];
}

public sealed record PublicProductCardViewModel(
    int Id,
    string ProductName,
    string CategoryName,
    decimal UnitPrice,
    bool HasPicture);

public sealed record PublicProductCategoryViewModel(int Id, string Name);
