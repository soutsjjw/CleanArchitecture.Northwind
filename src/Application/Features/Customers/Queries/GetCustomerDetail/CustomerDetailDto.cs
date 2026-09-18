namespace CleanArchitecture.Northwind.Application.Features.Customers.Queries.GetCustomerDetail;

public sealed record CustomerDetailDto(
    string Id,
    string CompanyName,
    string? ContactName,
    string? ContactTitle,
    string? Address,
    string? City,
    string? Region,
    string? PostalCode,
    string? Country,
    string? Phone,
    string? Fax);
