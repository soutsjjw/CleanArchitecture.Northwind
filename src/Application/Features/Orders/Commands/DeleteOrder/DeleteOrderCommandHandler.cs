using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Common.Models;

namespace CleanArchitecture.Northwind.Application.Features.Orders.Commands.DeleteOrder;

public class DeleteOrderCommandHandler(IApplicationDbContext context) : IRequestHandler<DeleteOrderCommand, Result>
{
    public async Task<Result> Handle(DeleteOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await context.Orders.FindAsync([request.Id], cancellationToken);
        if (order is null)
        {
            return await Result.FailureAsync("找不到訂單。");
        }

        order.IsDelete = true;
        await context.SaveChangesAsync(cancellationToken);

        return await Result.SuccessAsync();
    }
}
