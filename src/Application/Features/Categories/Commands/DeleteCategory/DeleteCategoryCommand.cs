using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Features.Categories.Commands.DeleteCategory;

public sealed record DeleteCategoryCommand : IRequest<Result>
{
    public int Id { get; init; }
}
