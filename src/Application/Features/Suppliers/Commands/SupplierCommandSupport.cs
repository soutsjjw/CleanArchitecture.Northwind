namespace CleanArchitecture.Northwind.Application.Features.Suppliers.Commands;

internal static class SupplierCommandSupport
{
    internal static string? ValidateInput(string? companyName, string? contactName, string? contactTitle, string? address, string? city, string? region, string? postalCode, string? country, string? phone, string? fax, string? homePage)
    {
        if (string.IsNullOrWhiteSpace(companyName))
            return "供應商公司名稱不可為空。";

        if (companyName.Trim().Length > 40)
            return "供應商公司名稱不可超過 40 個字元。";

        if (contactName?.Trim().Length > 30 || contactTitle?.Trim().Length > 30)
            return "聯絡人資料不可超過 30 個字元。";

        if (address?.Trim().Length > 60)
            return "地址不可超過 60 個字元。";

        if (city?.Trim().Length > 15 || region?.Trim().Length > 15 || country?.Trim().Length > 15)
            return "城市、地區或國家不可超過 15 個字元。";

        if (postalCode?.Trim().Length > 10)
            return "郵遞區號不可超過 10 個字元。";

        if (phone?.Trim().Length > 24 || fax?.Trim().Length > 24)
            return "電話或傳真不可超過 24 個字元。";

        if (!string.IsNullOrWhiteSpace(homePage)
            && (!Uri.TryCreate(homePage.Trim(), UriKind.Absolute, out var uri)
                || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)))
            return "網站首頁必須是 HTTP 或 HTTPS 網址。";

        return null;
    }
}
