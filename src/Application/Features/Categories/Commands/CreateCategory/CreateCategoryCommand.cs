using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Features.Categories.Commands.CreateCategory;

public sealed record CreateCategoryCommand : IRequest<Result<int>>
{
    public string CategoryName { get; init; } = string.Empty;

    public string? Description { get; init; }
}
