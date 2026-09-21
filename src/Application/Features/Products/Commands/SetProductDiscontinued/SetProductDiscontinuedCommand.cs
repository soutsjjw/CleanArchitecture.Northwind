using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Features.Products.Commands.SetProductDiscontinued;

public sealed record SetProductDiscontinuedCommand : IRequest<Result>
{
    public int Id { get; init; }

    public bool Discontinued { get; init; }
}
