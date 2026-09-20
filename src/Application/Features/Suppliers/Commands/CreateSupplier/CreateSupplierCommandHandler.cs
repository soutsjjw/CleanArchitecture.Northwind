using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Common.Extensions;
using CleanArchitecture.Northwind.Application.Common.Models;
using CleanArchitecture.Northwind.Domain.Entities;

namespace CleanArchitecture.Northwind.Application.Features.Suppliers.Commands.CreateSupplier;

public sealed class CreateSupplierCommandHandler(IApplicationDbContext context) : IRequestHandler<CreateSupplierCommand, Result<int>>
{
    public async Task<Result<int>> Handle(CreateSupplierCommand request, CancellationToken cancellationToken)
    {
        var error = SupplierCommandSupport.ValidateInput(request.CompanyName, request.ContactName, request.ContactTitle, request.Address, request.City, request.Region, request.PostalCode, request.Country, request.Phone, request.Fax, request.HomePage);
        if (error is not null)
            return Result<int>.Failure(error, 400);

        var name = request.CompanyName.Trim();
        if (await context.Suppliers.AnyAsync(x => !x.IsDelete && x.CompanyName.ToUpper() == name.ToUpper(), cancellationToken))
            return Result<int>.Failure("供應商公司名稱已存在。", 409);

        var supplier = new Supplier
        {
            CompanyName = name,
            ContactName = request.ContactName.TrimToNull(),
            ContactTitle = request.ContactTitle.TrimToNull(),
            Address = request.Address.TrimToNull(),
            City = request.City.TrimToNull(),
            Region = request.Region.TrimToNull(),
            PostalCode = request.PostalCode.TrimToNull(),
            Country = request.Country.TrimToNull(),
            Phone = request.Phone.TrimToNull(),
            Fax = request.Fax.TrimToNull(),
            HomePage = request.HomePage.TrimToNull(),
            IsActive = true
        };

        context.Suppliers.Add(supplier);

        await context.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(supplier.Id);
    }
}
