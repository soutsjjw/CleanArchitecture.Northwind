using System.Collections;
using System.Linq.Expressions;
using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Features.Customers.Queries.GetCustomerOrderHistory;
using CleanArchitecture.Northwind.Application.Features.Orders.Queries.GetOrders;
using CleanArchitecture.Northwind.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace CleanArchitecture.Northwind.Application.UnitTests.Features.Customers.Queries.GetCustomerOrderHistory;

public class GetCustomerOrderHistoryQueryHandlerTests
{
    [Test]
    public async Task HandleShouldReturnOnlyTheCustomersActiveOrdersInDescendingOrderDatePages()
    {
        var context = new Mock<IApplicationDbContext>();
        context.Setup(value => value.Orders).Returns(CreateDbSet(new[]
        {
            new Order
            {
                Id = 10248,
                CustomerId = "ALFKI",
                OrderDate = new DateTime(2026, 1, 10),
                ShippedDate = new DateTime(2026, 1, 12),
                Shipper = new Shipper { CompanyName = "Speedy Express" },
                OrderDetails = { new OrderDetail { UnitPrice = 10m, Quantity = 2 } }
            },
            new Order
            {
                Id = 10249,
                CustomerId = "ALFKI",
                OrderDate = new DateTime(2026, 1, 11),
                RequiredDate = DateTime.Today.AddDays(1),
                Shipper = new Shipper { CompanyName = "United Package" },
                OrderDetails = { new OrderDetail { UnitPrice = 5m, Quantity = 3 } }
            },
            new Order
            {
                Id = 10250,
                CustomerId = "ANATR",
                OrderDate = new DateTime(2026, 1, 12),
                OrderDetails = { new OrderDetail { UnitPrice = 100m, Quantity = 1 } }
            },
            new Order
            {
                Id = 10251,
                CustomerId = "ALFKI",
                OrderDate = new DateTime(2026, 1, 13),
                IsDelete = true,
                OrderDetails = { new OrderDetail { UnitPrice = 100m, Quantity = 1 } }
            }
        }));
        var handler = new GetCustomerOrderHistoryQueryHandler(context.Object);

        var result = await handler.Handle(new GetCustomerOrderHistoryQuery("ALFKI")
        {
            PageNumber = 2,
            PageSize = 1
        }, CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        result.Data.Orders.TotalCount.ShouldBe(2);
        var order = result.Data.Orders.Items.Single();
        order.Id.ShouldBe(10248);
        order.TotalAmount.ShouldBe(20m);
        order.ShipperName.ShouldBe("Speedy Express");
        order.ShippingStatus.ShouldBe(OrderShippingStatus.Shipped);
    }

    private static DbSet<T> CreateDbSet<T>(IEnumerable<T> source)
        where T : class
    {
        var queryable = source.AsQueryable();
        var dbSet = new Mock<DbSet<T>>();

        dbSet.As<IAsyncEnumerable<T>>()
            .Setup(value => value.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(new TestAsyncEnumerator<T>(queryable.GetEnumerator()));
        dbSet.As<IQueryable<T>>().Setup(value => value.Provider)
            .Returns(new TestAsyncQueryProvider<T>(queryable.Provider));
        dbSet.As<IQueryable<T>>().Setup(value => value.Expression).Returns(queryable.Expression);
        dbSet.As<IQueryable<T>>().Setup(value => value.ElementType).Returns(queryable.ElementType);
        dbSet.As<IQueryable<T>>().Setup(value => value.GetEnumerator())
            .Returns(() => queryable.GetEnumerator());

        return dbSet.Object;
    }

    private sealed class TestAsyncQueryProvider<TEntity>(IQueryProvider inner) : IAsyncQueryProvider
    {
        public IQueryable CreateQuery(Expression expression) => new TestAsyncEnumerable<TEntity>(expression);
        public IQueryable<TElement> CreateQuery<TElement>(Expression expression) => new TestAsyncEnumerable<TElement>(expression);
        public object? Execute(Expression expression) => inner.Execute(expression);
        public TResult Execute<TResult>(Expression expression) => inner.Execute<TResult>(expression);

        public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
        {
            var resultType = typeof(TResult).GetGenericArguments()[0];
            var result = typeof(IQueryProvider).GetMethod(nameof(IQueryProvider.Execute), 1, [typeof(Expression)])!
                .MakeGenericMethod(resultType).Invoke(inner, [expression]);
            return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))!
                .MakeGenericMethod(resultType).Invoke(null, [result])!;
        }
    }

    private sealed class TestAsyncEnumerable<T>(Expression expression)
        : EnumerableQuery<T>(expression), IAsyncEnumerable<T>, IQueryable<T>
    {
        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            => new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());

        IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
    }

    private sealed class TestAsyncEnumerator<T>(IEnumerator<T> inner) : IAsyncEnumerator<T>
    {
        public T Current => inner.Current;
        public ValueTask DisposeAsync() { inner.Dispose(); return ValueTask.CompletedTask; }
        public ValueTask<bool> MoveNextAsync() => new(inner.MoveNext());
    }
}
