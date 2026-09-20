using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Common.Extensions;
using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Features.Suppliers.Commands.UpdateSupplier;

public sealed class UpdateSupplierCommandHandler(IApplicationDbContext context) : IRequestHandler<UpdateSupplierCommand, Result>
{
    public async Task<Result> Handle(UpdateSupplierCommand request, CancellationToken cancellationToken)
    {
        if (request.Id <= 0)
            return await Result.FailureAsync("供應商編號無效。", 400);

        var error = SupplierCommandSupport.ValidateInput(request.CompanyName, request.ContactName, request.ContactTitle, request.Address, request.City, request.Region, request.PostalCode, request.Country, request.Phone, request.Fax, request.HomePage);
        if (error is not null)
            return await Result.FailureAsync(error, 400);

        var supplier = await context.Suppliers.FindAsync([request.Id], cancellationToken);
        if (supplier is null || supplier.IsDelete)
            return await Result.FailureAsync("找不到供應商。", 404);

        var name = request.CompanyName.Trim();
        if (await context.Suppliers.AnyAsync(x => !x.IsDelete && x.Id != request.Id && x.CompanyName.ToUpper() == name.ToUpper(), cancellationToken))
            return await Result.FailureAsync("供應商公司名稱已存在。", 409);

        supplier.CompanyName = name;
        supplier.ContactName = request.ContactName.TrimToNull();
        supplier.ContactTitle = request.ContactTitle.TrimToNull();
        supplier.Address = request.Address.TrimToNull();
        supplier.City = request.City.TrimToNull();
        supplier.Region = request.Region.TrimToNull();
        supplier.PostalCode = request.PostalCode.TrimToNull();
        supplier.Country = request.Country.TrimToNull();
        supplier.Phone = request.Phone.TrimToNull();
        supplier.Fax = request.Fax.TrimToNull();
        supplier.HomePage = request.HomePage.TrimToNull();

        await context.SaveChangesAsync(cancellationToken);

        return await Result.SuccessAsync();
    }
}
