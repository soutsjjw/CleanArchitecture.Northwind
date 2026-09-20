using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Features.Suppliers.Commands.SetSupplierActive;

public sealed record SetSupplierActiveCommand(int Id, bool IsActive) : IRequest<Result>;
