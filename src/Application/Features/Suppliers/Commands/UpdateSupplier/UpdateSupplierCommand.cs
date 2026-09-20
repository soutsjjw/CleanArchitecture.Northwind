using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Common.Models;
using CleanArchitecture.Northwind.Application.Features.Suppliers.Commands;

namespace CleanArchitecture.Northwind.Application.Features.Suppliers.Commands.UpdateSupplier;

public sealed record UpdateSupplierCommand : IRequest<Result>
{
    public int Id { get; init; } public string CompanyName { get; init; } = string.Empty; public string? ContactName { get; init; } public string? ContactTitle { get; init; } public string? Address { get; init; } public string? City { get; init; } public string? Region { get; init; } public string? PostalCode { get; init; } public string? Country { get; init; } public string? Phone { get; init; } public string? Fax { get; init; } public string? HomePage { get; init; }
}

public sealed class UpdateSupplierCommandHandler(IApplicationDbContext context) : IRequestHandler<UpdateSupplierCommand, Result>
{
    public async Task<Result> Handle(UpdateSupplierCommand request, CancellationToken cancellationToken)
    {
        if (request.Id <= 0) return Result.Failure("供應商編號無效。", 400);
        var error = SupplierCommandSupport.ValidateInput(request.CompanyName, request.ContactName, request.ContactTitle, request.Address, request.City, request.Region, request.PostalCode, request.Country, request.Phone, request.Fax, request.HomePage);
        if (error is not null) return Result.Failure(error, 400);
        var supplier = await context.Suppliers.FindAsync([request.Id], cancellationToken);
        if (supplier is null || supplier.IsDelete) return Result.Failure("找不到供應商。", 404);
        var name = request.CompanyName.Trim();
        if (await context.Suppliers.AnyAsync(x => !x.IsDelete && x.Id != request.Id && x.CompanyName.ToUpper() == name.ToUpper(), cancellationToken)) return Result.Failure("供應商公司名稱已存在。", 409);
        supplier.CompanyName = name; supplier.ContactName = SupplierCommandSupport.TrimToNull(request.ContactName); supplier.ContactTitle = SupplierCommandSupport.TrimToNull(request.ContactTitle); supplier.Address = SupplierCommandSupport.TrimToNull(request.Address); supplier.City = SupplierCommandSupport.TrimToNull(request.City); supplier.Region = SupplierCommandSupport.TrimToNull(request.Region); supplier.PostalCode = SupplierCommandSupport.TrimToNull(request.PostalCode); supplier.Country = SupplierCommandSupport.TrimToNull(request.Country); supplier.Phone = SupplierCommandSupport.TrimToNull(request.Phone); supplier.Fax = SupplierCommandSupport.TrimToNull(request.Fax); supplier.HomePage = SupplierCommandSupport.TrimToNull(request.HomePage);
        await context.SaveChangesAsync(cancellationToken); return Result.Success();
    }
}
