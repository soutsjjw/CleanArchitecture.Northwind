namespace CleanArchitecture.Northwind.Application.Features.Inventory.Commands.AdjustInventory;

public sealed class AdjustInventoryCommandValidator : AbstractValidator<AdjustInventoryCommand>
{
    public AdjustInventoryCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .GreaterThan(0)
            .WithMessage("商品編號必須大於 0。");

        RuleFor(x => x.Reason)
            .Must(reason => !string.IsNullOrWhiteSpace(reason))
            .WithMessage("調整原因不可為空。")
            .Must(reason => reason is null || reason.Trim().Length <= 250)
            .WithMessage("調整原因不可超過 250 個字元。");

        RuleFor(x => x.RowVersion)
            .NotEmpty()
            .WithMessage("庫存版本不可為空。");
    }
}
