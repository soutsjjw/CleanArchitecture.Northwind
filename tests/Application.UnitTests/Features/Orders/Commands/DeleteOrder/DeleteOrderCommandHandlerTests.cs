using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Features.Orders.Commands.DeleteOrder;
using CleanArchitecture.Northwind.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace CleanArchitecture.Northwind.Application.UnitTests.Features.Orders.Commands.DeleteOrder;

public class DeleteOrderCommandHandlerTests
{
    [Test]
    public async Task HandleShouldMarkOrderAsDeleted()
    {
        var order = new Order { Id = 10248 };
        var orders = new Mock<DbSet<Order>>();
        var context = new Mock<IApplicationDbContext>();
        context.Setup(x => x.Orders).Returns(orders.Object);
        orders.Setup(x => x.FindAsync(new object[] { order.Id }, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);
        context.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new DeleteOrderCommandHandler(context.Object);

        var result = await handler.Handle(new DeleteOrderCommand { Id = order.Id }, CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        order.IsDelete.ShouldBeTrue();
        orders.Verify(x => x.Remove(It.IsAny<Order>()), Times.Never);
        context.Verify(x => x.SaveChangesAsync(CancellationToken.None), Times.Once);
    }
}
