namespace CleanArchitecture.Northwind.Application.Features.Suppliers.Queries.GetSuppliers;

public sealed record SupplierListItemDto(
    int Id,
    string CompanyName,
    string? ContactName,
    string? Phone,
    string? Country,
    bool IsActive,
    int ProductCount);
