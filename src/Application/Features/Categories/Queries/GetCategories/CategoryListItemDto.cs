namespace CleanArchitecture.Northwind.Application.Features.Categories.Queries.GetCategories;

public sealed record CategoryListItemDto(
    int Id,
    string CategoryName,
    string? Description,
    bool IsActive,
    int ProductCount);
