using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Common.Models;
using CleanArchitecture.Northwind.Application.Features.Products.Commands;

namespace CleanArchitecture.Northwind.Application.Features.Products.Commands.UpdateProduct;

public sealed record UpdateProductCommand : IRequest<Result>
{
    public int Id { get; init; }

    public string ProductName { get; init; } = string.Empty;

    public int CategoryId { get; init; }

    public int SupplierId { get; init; }

    public string? QuantityPerUnit { get; init; }

    public decimal? UnitPrice { get; init; }

    public short? ReorderLevel { get; init; }

    public byte[]? Picture { get; init; }

    public string? PictureContentType { get; init; }

    public bool RemovePicture { get; init; }
}

public sealed class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(command => command.Id)
            .GreaterThan(0)
            .WithMessage("商品編號無效。");

        RuleFor(command => command.ProductName)
            .Must(name => !string.IsNullOrWhiteSpace(name))
            .WithMessage("商品名稱不可為空。")
            .Must(name => name is null || name.Trim().Length <= 40)
            .WithMessage("商品名稱不可超過 40 個字元。");

        RuleFor(command => command.CategoryId)
            .GreaterThan(0)
            .WithMessage("商品分類無效。");

        RuleFor(command => command.SupplierId)
            .GreaterThan(0)
            .WithMessage("供應商無效。");

        RuleFor(command => command.QuantityPerUnit)
            .Must(value => value is null || value.Trim().Length <= 20)
            .WithMessage("包裝量不可超過 20 個字元。");

        RuleFor(command => command.UnitPrice)
            .GreaterThanOrEqualTo(0)
            .When(command => command.UnitPrice.HasValue)
            .WithMessage("商品單價不可小於 0。");

        RuleFor(command => command.ReorderLevel)
            .GreaterThanOrEqualTo((short)0)
            .When(command => command.ReorderLevel.HasValue)
            .WithMessage("再訂購水準不可小於 0。");

        RuleFor(command => command)
            .Must(command =>
                (command.Picture is null) == string.IsNullOrWhiteSpace(command.PictureContentType))
            .WithMessage("商品圖片與內容類型必須同時提供。")
            .Must(command => !command.RemovePicture || command.Picture is null)
            .WithMessage("移除圖片時不可同時上傳新圖片。");
    }
}

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
            cancellationToken);
        if (referenceError is not null)
        {
            return Result.Failure(referenceError, 400);
        }

        product.ProductName = request.ProductName.Trim();
        product.CategoryId = request.CategoryId;
        product.SupplierId = request.SupplierId;
        product.QuantityPerUnit = ProductCommandSupport.TrimToNull(request.QuantityPerUnit);
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
                ProductCommandSupport.TrimToNull(request.PictureContentType);
        }

        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
