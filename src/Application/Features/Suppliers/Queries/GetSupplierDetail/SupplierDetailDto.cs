namespace CleanArchitecture.Northwind.Application.Features.Suppliers.Queries.GetSupplierDetail;

public sealed record SupplierDetailDto(
    int Id,
    string CompanyName,
    string? ContactName,
    string? ContactTitle,
    string? Address,
    string? City,
    string? Region,
    string? PostalCode,
    string? Country,
    string? Phone,
    string? Fax,
    string? HomePage,
    bool IsActive,
    int ProductCount);
