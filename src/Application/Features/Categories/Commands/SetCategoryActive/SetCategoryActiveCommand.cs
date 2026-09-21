using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Features.Categories.Commands.SetCategoryActive;

public sealed record SetCategoryActiveCommand : IRequest<Result>
{
    public int Id { get; init; }

    public bool IsActive { get; init; }
}
