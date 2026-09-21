using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Features.Categories.Commands.UpdateCategory;

public sealed record UpdateCategoryCommand : IRequest<Result>
{
    public int Id { get; init; }

    public string CategoryName { get; init; } = string.Empty;

    public string? Description { get; init; }
}
