namespace CleanArchitecture.Northwind.Application.Features.Categories.Commands;

internal static class CategoryCommandSupport
{
    internal static string? ValidateInput(string? categoryName)
    {
        if (string.IsNullOrWhiteSpace(categoryName))
        {
            return "分類名稱不可為空。";
        }

        return categoryName.Trim().Length <= 15
            ? null
            : "分類名稱不可超過 15 個字元。";
    }

    internal static string? TrimToNull(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
