using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Features.Products.Commands.CreateProduct;

public sealed record CreateProductCommand : IRequest<Result<int>>
{
    public string ProductName { get; init; } = string.Empty;

    public int CategoryId { get; init; }

    public int SupplierId { get; init; }

    public string? QuantityPerUnit { get; init; }

    public decimal? UnitPrice { get; init; }

    public short? ReorderLevel { get; init; }

    public byte[]? Picture { get; init; }

    public string? PictureContentType { get; init; }
}
