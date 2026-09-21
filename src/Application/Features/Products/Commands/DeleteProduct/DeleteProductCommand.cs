using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Features.Products.Commands.DeleteProduct;

public sealed record DeleteProductCommand : IRequest<Result>
{
    public int Id { get; init; }
}
