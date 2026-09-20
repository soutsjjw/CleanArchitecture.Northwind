using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Features.Suppliers.Commands.DeleteSupplier;

public sealed record DeleteSupplierCommand : IRequest<Result>
{
    public int Id { get; init; }
}
