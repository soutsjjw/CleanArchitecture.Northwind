using CleanArchitecture.Northwind.Application.Common.Extensions;
using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Common.Models;
using CleanArchitecture.Northwind.Domain.Entities;

namespace CleanArchitecture.Northwind.Application.Features.Products.Commands.CreateProduct;

public sealed class CreateProductCommandHandler(IApplicationDbContext context)
    : IRequestHandler<CreateProductCommand, Result<int>>
{
    public async Task<Result<int>> Handle(
        CreateProductCommand request,
        CancellationToken cancellationToken)
    {
        var inputError = ProductCommandSupport.ValidateInput(
            request.ProductName,
            request.CategoryId,
            request.SupplierId,
            request.QuantityPerUnit,
            request.UnitPrice,
            request.ReorderLevel,
            request.Picture,
            request.PictureContentType);
        if (inputError is not null)
        {
            return Result<int>.Failure(inputError, 400);
        }

        var referenceError = await ProductCommandSupport.ValidateReferencesAsync(
            context,
            request.CategoryId,
            request.SupplierId,
            cancellationToken);
        if (referenceError is not null)
        {
            return Result<int>.Failure(referenceError, 400);
        }

        var product = new Product
        {
            ProductName = request.ProductName.Trim(),
            CategoryId = request.CategoryId,
            SupplierId = request.SupplierId,
            QuantityPerUnit = request.QuantityPerUnit.TrimToNull(),
            UnitPrice = request.UnitPrice,
            UnitsInStock = 0,
            UnitsOnOrder = 0,
            ReorderLevel = request.ReorderLevel ?? 0,
            Discontinued = false,
            Picture = request.Picture?.ToArray(),
            PictureContentType = request.PictureContentType.TrimToNull()
        };

        context.Products.Add(product);
        await context.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(product.Id);
    }
}
