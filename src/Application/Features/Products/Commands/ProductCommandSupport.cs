using CleanArchitecture.Northwind.Application.Common.Interfaces;

namespace CleanArchitecture.Northwind.Application.Features.Products.Commands;

internal static class ProductCommandSupport
{
    internal static string? ValidateInput(
        string? productName,
        int categoryId,
        int supplierId,
        string? quantityPerUnit,
        decimal? unitPrice,
        short? reorderLevel,
        byte[]? picture,
        string? pictureContentType)
    {
        if (string.IsNullOrWhiteSpace(productName))
        {
            return "商品名稱不可為空。";
        }

        if (productName.Trim().Length > 40)
        {
            return "商品名稱不可超過 40 個字元。";
        }

        if (categoryId <= 0)
        {
            return "商品分類無效。";
        }

        if (supplierId <= 0)
        {
            return "供應商無效。";
        }

        if (quantityPerUnit?.Trim().Length > 20)
        {
            return "包裝量不可超過 20 個字元。";
        }

        if (unitPrice < 0)
        {
            return "商品單價不可小於 0。";
        }

        if (reorderLevel < 0)
        {
            return "再訂購水準不可小於 0。";
        }

        if ((picture is null) != string.IsNullOrWhiteSpace(pictureContentType))
        {
            return "商品圖片與內容類型必須同時提供。";
        }

        return null;
    }

    internal static async Task<string?> ValidateReferencesAsync(
        IApplicationDbContext context,
        int categoryId,
        int supplierId,
        CancellationToken cancellationToken)
    {
        var categoryIsSelectable = await context.Categories
            .AnyAsync(
                category => category.Id == categoryId
                    && !category.IsDelete
                    && category.IsActive,
                cancellationToken);
        if (!categoryIsSelectable)
        {
            return "商品分類不存在或已停用。";
        }

        var supplierIsSelectable = await context.Suppliers
            .AnyAsync(
                supplier => supplier.Id == supplierId
                    && !supplier.IsDelete
                    && supplier.IsActive,
                cancellationToken);
        return supplierIsSelectable
            ? null
            : "供應商不存在或已停用。";
    }

    internal static string? TrimToNull(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
