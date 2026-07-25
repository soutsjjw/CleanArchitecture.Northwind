using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Features.Products.Queries.GetProductOptions;

public sealed record GetProductOptionsQuery
    : IRequest<Result<IReadOnlyList<ProductOptionDto>>>;

public sealed record ProductOptionDto(int Id, string Name);
