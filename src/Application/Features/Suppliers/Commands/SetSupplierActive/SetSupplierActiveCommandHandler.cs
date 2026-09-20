using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Features.Suppliers.Commands.SetSupplierActive;

public sealed class SetSupplierActiveCommandHandler(IApplicationDbContext context) : IRequestHandler<SetSupplierActiveCommand, Result>
{
    public async Task<Result> Handle(SetSupplierActiveCommand request, CancellationToken cancellationToken)
    {
        if (request.Id <= 0)
            return await Result.FailureAsync("供應商編號無效。", 400);

        var supplier = await context.Suppliers.FindAsync([request.Id], cancellationToken);
        if (supplier is null || supplier.IsDelete)
            return await Result.FailureAsync("找不到供應商。", 404);

        supplier.IsActive = request.IsActive;

        await context.SaveChangesAsync(cancellationToken);

        return await Result.SuccessAsync();
    }
}
