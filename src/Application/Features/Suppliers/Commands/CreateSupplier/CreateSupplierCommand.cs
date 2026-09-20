using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Features.Suppliers.Commands.CreateSupplier;

public sealed record CreateSupplierCommand : IRequest<Result<int>>
{
    public string CompanyName { get; init; } = string.Empty;
    public string? ContactName { get; init; }
    public string? ContactTitle { get; init; }
    public string? Address { get; init; }
    public string? City { get; init; }
    public string? Region { get; init; }
    public string? PostalCode { get; init; }
    public string? Country { get; init; }
    public string? Phone { get; init; }
    public string? Fax { get; init; }
    public string? HomePage { get; init; }
}
