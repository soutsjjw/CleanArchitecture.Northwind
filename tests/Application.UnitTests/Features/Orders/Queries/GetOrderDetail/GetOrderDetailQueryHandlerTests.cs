using System.Collections;
using System.Linq.Expressions;
using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Features.Orders.Queries.GetOrderDetail;
using CleanArchitecture.Northwind.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace CleanArchitecture.Northwind.Application.UnitTests.Features.Orders.Queries.GetOrderDetail;

public class GetOrderDetailQueryHandlerTests
{
    [Test]
    public async Task HandleShouldReturnOrderHeaderAndLineItems()
    {
        var context = new Mock<IApplicationDbContext>();
        context.Setup(x => x.Orders).Returns(CreateDbSet(new[]
        {
            new Order
            {
                Id = 10248,
                CustomerId = "ALFKI",
                Customer = new Customer { Id = "ALFKI", CompanyName = "Alfreds Futterkiste" },
                Employee = new Employee { FirstName = "Nancy", LastName = "Davolio", Notes = "Sales" },
                OrderDate = new DateTime(2026, 1, 10),
                RequiredDate = new DateTime(2026, 1, 20),
                Shipper = new Shipper { CompanyName = "Speedy Express" },
                Freight = 32.38m,
                ShipCity = "Berlin",
                ShipCountry = "Germany",
                OrderDetails =
                {
                    new OrderDetail
                    {
                        OrderId = 10248,
                        ProductId = 11,
                        Product = new Product { ProductName = "Queso Cabrales" },
                        UnitPrice = 14m,
                        Quantity = 12,
                        Discount = 0
                    }
                }
            }
        }));

        var handler = new GetOrderDetailQueryHandler(context.Object);

        var result = await handler.Handle(new GetOrderDetailQuery { Id = 10248 }, CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        result.Data.Id.ShouldBe(10248);
        result.Data.CustomerName.ShouldBe("Alfreds Futterkiste");
        result.Data.Items.ShouldHaveSingleItem();
        result.Data.Items[0].ProductName.ShouldBe("Queso Cabrales");
        result.Data.Items[0].TotalAmount.ShouldBe(168m);
    }

    private static DbSet<T> CreateDbSet<T>(IEnumerable<T> source) where T : class
    {
        var queryable = source.AsQueryable();
        var dbSet = new Mock<DbSet<T>>();
        dbSet.As<IAsyncEnumerable<T>>().Setup(x => x.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(new TestAsyncEnumerator<T>(queryable.GetEnumerator()));
        dbSet.As<IQueryable<T>>().Setup(x => x.Provider).Returns(new TestAsyncQueryProvider<T>(queryable.Provider));
        dbSet.As<IQueryable<T>>().Setup(x => x.Expression).Returns(queryable.Expression);
        dbSet.As<IQueryable<T>>().Setup(x => x.ElementType).Returns(queryable.ElementType);
        dbSet.As<IQueryable<T>>().Setup(x => x.GetEnumerator()).Returns(() => queryable.GetEnumerator());
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

    private sealed class TestAsyncEnumerable<T>(Expression expression) : EnumerableQuery<T>(expression), IAsyncEnumerable<T>, IQueryable<T>
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
