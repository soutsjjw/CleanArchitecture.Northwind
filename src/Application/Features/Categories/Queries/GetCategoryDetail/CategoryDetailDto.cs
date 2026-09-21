namespace CleanArchitecture.Northwind.Application.Features.Categories.Queries.GetCategoryDetail;

public sealed record CategoryDetailDto(
    int Id,
    string CategoryName,
    string? Description,
    bool IsActive,
    int ProductCount);
