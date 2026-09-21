namespace CleanArchitecture.Northwind.Application.Features.Products.Commands.CreateProduct;

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
