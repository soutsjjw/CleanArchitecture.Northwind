using CleanArchitecture.Northwind.Application.Common.Extensions;
using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Features.Products.Commands.UpdateProduct;

public sealed class UpdateProductCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateProductCommand, Result>
{
    public async Task<Result> Handle(
        UpdateProductCommand request,
        CancellationToken cancellationToken)
    {
        if (request.Id <= 0)
        {
            return Result.Failure("商品編號無效。", 400);
        }

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
            return Result.Failure(inputError, 400);
        }

        if (request.RemovePicture && request.Picture is not null)
        {
            return Result.Failure("移除圖片時不可同時上傳新圖片。", 400);
        }

        var product = await context.Products.FindAsync([request.Id], cancellationToken);
        if (product is null || product.IsDelete)
        {
            return Result.Failure("找不到商品。", 404);
        }

        var referenceError = await ProductCommandSupport.ValidateReferencesAsync(
            context,
            request.CategoryId,
            request.SupplierId,
            cancellationToken,
            product.SupplierId);
        if (referenceError is not null)
        {
            return Result.Failure(referenceError, 400);
        }

        product.ProductName = request.ProductName.Trim();
        product.CategoryId = request.CategoryId;
        product.SupplierId = request.SupplierId;
        product.QuantityPerUnit = request.QuantityPerUnit.TrimToNull();
        product.UnitPrice = request.UnitPrice;
        product.ReorderLevel = request.ReorderLevel ?? 0;

        if (request.RemovePicture)
        {
            product.Picture = null;
            product.PictureContentType = null;
        }
        else if (request.Picture is not null)
        {
            product.Picture = request.Picture.ToArray();
            product.PictureContentType =
                request.PictureContentType.TrimToNull();
        }

        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
