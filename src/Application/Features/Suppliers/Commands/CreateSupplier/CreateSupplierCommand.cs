using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Common.Models;
using CleanArchitecture.Northwind.Application.Features.Suppliers.Commands;
using CleanArchitecture.Northwind.Domain.Entities;

namespace CleanArchitecture.Northwind.Application.Features.Suppliers.Commands.CreateSupplier;

public sealed record CreateSupplierCommand : IRequest<Result<int>>
{
    public string CompanyName { get; init; } = string.Empty; public string? ContactName { get; init; } public string? ContactTitle { get; init; } public string? Address { get; init; } public string? City { get; init; } public string? Region { get; init; } public string? PostalCode { get; init; } public string? Country { get; init; } public string? Phone { get; init; } public string? Fax { get; init; } public string? HomePage { get; init; }
}

public sealed class CreateSupplierCommandHandler(IApplicationDbContext context) : IRequestHandler<CreateSupplierCommand, Result<int>>
{
    public async Task<Result<int>> Handle(CreateSupplierCommand request, CancellationToken cancellationToken)
    {
        var error = SupplierCommandSupport.ValidateInput(request.CompanyName, request.ContactName, request.ContactTitle, request.Address, request.City, request.Region, request.PostalCode, request.Country, request.Phone, request.Fax, request.HomePage);
        if (error is not null) return Result<int>.Failure(error, 400);
        var name = request.CompanyName.Trim();
        if (await context.Suppliers.AnyAsync(x => !x.IsDelete && x.CompanyName.ToUpper() == name.ToUpper(), cancellationToken)) return Result<int>.Failure("供應商公司名稱已存在。", 409);
        var supplier = new Supplier { CompanyName = name, ContactName = SupplierCommandSupport.TrimToNull(request.ContactName), ContactTitle = SupplierCommandSupport.TrimToNull(request.ContactTitle), Address = SupplierCommandSupport.TrimToNull(request.Address), City = SupplierCommandSupport.TrimToNull(request.City), Region = SupplierCommandSupport.TrimToNull(request.Region), PostalCode = SupplierCommandSupport.TrimToNull(request.PostalCode), Country = SupplierCommandSupport.TrimToNull(request.Country), Phone = SupplierCommandSupport.TrimToNull(request.Phone), Fax = SupplierCommandSupport.TrimToNull(request.Fax), HomePage = SupplierCommandSupport.TrimToNull(request.HomePage), IsActive = true };
        context.Suppliers.Add(supplier); await context.SaveChangesAsync(cancellationToken); return Result<int>.Success(supplier.Id);
    }
}
