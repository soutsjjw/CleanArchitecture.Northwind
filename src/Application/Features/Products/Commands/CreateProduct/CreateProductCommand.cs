using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Common.Models;
using CleanArchitecture.Northwind.Application.Features.Products.Commands;
using CleanArchitecture.Northwind.Domain.Entities;

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

public sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
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
            .WithMessage("商品圖片與內容類型必須同時提供。");
    }
}

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
            QuantityPerUnit = ProductCommandSupport.TrimToNull(request.QuantityPerUnit),
            UnitPrice = request.UnitPrice,
            UnitsInStock = 0,
            UnitsOnOrder = 0,
            ReorderLevel = request.ReorderLevel ?? 0,
            Discontinued = false,
            Picture = request.Picture?.ToArray(),
            PictureContentType = ProductCommandSupport.TrimToNull(request.PictureContentType)
        };

        context.Products.Add(product);
        await context.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(product.Id);
    }
}
