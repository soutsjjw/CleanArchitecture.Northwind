namespace CleanArchitecture.Northwind.Application.Features.Suppliers.Queries.GetSupplierDetail;

public sealed record SupplierSuppliedProductDto(
    int Id,
    string ProductName,
    string? QuantityPerUnit,
    decimal? UnitPrice,
    bool Discontinued);
