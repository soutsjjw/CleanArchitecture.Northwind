namespace CleanArchitecture.Northwind.Application.Common.Extensions;

public static class StringExtensions
{
    public static string? TrimToNull(this string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
