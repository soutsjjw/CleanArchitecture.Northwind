using System.Collections;
using System.Linq.Expressions;
using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Features.Orders.Queries.GetOrders;
using CleanArchitecture.Northwind.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace CleanArchitecture.Northwind.Application.UnitTests.Features.Orders.Queries.GetOrders;

public class GetOrdersQueryHandlerTests
{
    [Test]
    public async Task HandleShouldFilterUnshippedOrdersByKeywordAndReturnProjectedTotals()
    {
        var context = new Mock<IApplicationDbContext>();
        context.Setup(x => x.Orders).Returns(CreateDbSet(new[]
        {
            new Order
            {
                Id = 10248,
                CustomerId = "ALFKI",
                Customer = new Customer { Id = "ALFKI", CompanyName = "Alfreds Futterkiste", Country = "Germany" },
                EmployeeId = 1,
                Employee = new Employee { Id = 1, FirstName = "Nancy", LastName = "Davolio", Notes = "Sales" },
                ShipVia = 1,
                Shipper = new Shipper { Id = 1, CompanyName = "Speedy Express" },
                OrderDate = new DateTime(2026, 1, 10),
                RequiredDate = DateTime.Today.AddDays(10),
                ShippedDate = null,
                Freight = 32.38m,
                ShipCountry = "Germany",
                ShipCity = "Berlin",
                OrderDetails =
                {
                    new OrderDetail { OrderId = 10248, ProductId = 11, UnitPrice = 14m, Quantity = 12, Discount = 0 },
                    new OrderDetail { OrderId = 10248, ProductId = 42, UnitPrice = 9.8m, Quantity = 10, Discount = 0.1f }
                }
            },
            new Order
            {
                Id = 10249,
                CustomerId = "BONAP",
                Customer = new Customer { Id = "BONAP", CompanyName = "Bon app", Country = "France" },
                EmployeeId = 2,
                Employee = new Employee { Id = 2, FirstName = "Andrew", LastName = "Fuller", Notes = "Sales" },
                ShippedDate = new DateTime(2026, 1, 12),
                OrderDate = new DateTime(2026, 1, 11),
                OrderDetails =
                {
                    new OrderDetail { OrderId = 10249, ProductId = 14, UnitPrice = 18.6m, Quantity = 9, Discount = 0 }
                }
            }
        }));

        var handler = new GetOrdersQueryHandler(context.Object);

        var result = await handler.Handle(new GetOrdersQuery
        {
            Keyword = "alf",
            ShippingStatus = OrderShippingStatus.Unshipped,
            PageNumber = 1,
            PageSize = 10
        }, CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        result.Data.Keyword.ShouldBe("alf");
        result.Data.ShippingStatus.ShouldBe(OrderShippingStatus.Unshipped);
        result.Data.Orders.TotalCount.ShouldBe(1);

        var order = result.Data.Orders.Items.Single();
        order.Id.ShouldBe(10248);
        order.CustomerName.ShouldBe("Alfreds Futterkiste");
        order.EmployeeName.ShouldBe("Nancy Davolio");
        order.ShipperName.ShouldBe("Speedy Express");
        order.ShippingStatus.ShouldBe(OrderShippingStatus.Unshipped);
        order.LineCount.ShouldBe(2);
        order.TotalAmount.ShouldBe(256.20m);
    }

    [Test]
    public async Task HandleShouldExcludeSoftDeletedOrders()
    {
        var context = new Mock<IApplicationDbContext>();
        context.Setup(x => x.Orders).Returns(CreateDbSet(new[]
        {
            new Order { Id = 10248, Customer = new Customer { CompanyName = "Active" }, OrderDate = new DateTime(2026, 1, 10) },
            new Order { Id = 10249, Customer = new Customer { CompanyName = "Deleted" }, OrderDate = new DateTime(2026, 1, 11), IsDelete = true }
        }));
        var handler = new GetOrdersQueryHandler(context.Object);

        var result = await handler.Handle(new GetOrdersQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        result.Data.Orders.TotalCount.ShouldBe(1);
        result.Data.Orders.Items.Single().Id.ShouldBe(10248);
    }

    [Test]
    public async Task HandleShouldSortOrdersByCustomerName()
    {
        var context = new Mock<IApplicationDbContext>();
        context.Setup(x => x.Orders).Returns(CreateDbSet(new[]
        {
            new Order { Id = 10248, Customer = new Customer { CompanyName = "Zebra" }, OrderDate = new DateTime(2026, 1, 10) },
            new Order { Id = 10249, Customer = new Customer { CompanyName = "Alpha" }, OrderDate = new DateTime(2026, 1, 11) }
        }));
        var handler = new GetOrdersQueryHandler(context.Object);

        var result = await handler.Handle(new GetOrdersQuery
        {
            PageNumber = 1,
            PageSize = 10,
            SortBy = OrderSortField.CustomerName,
            SortDescending = false
        }, CancellationToken.None);

        result.Data.Orders.Items.Select(x => x.Id).ShouldBe([10249, 10248]);
    }

    private static DbSet<T> CreateDbSet<T>(IEnumerable<T> source)
        where T : class
    {
        var queryable = source.AsQueryable();
        var dbSet = new Mock<DbSet<T>>();

        dbSet.As<IAsyncEnumerable<T>>()
            .Setup(x => x.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(new TestAsyncEnumerator<T>(queryable.GetEnumerator()));
        dbSet.As<IQueryable<T>>()
            .Setup(x => x.Provider)
            .Returns(new TestAsyncQueryProvider<T>(queryable.Provider));
        dbSet.As<IQueryable<T>>().Setup(x => x.Expression).Returns(queryable.Expression);
        dbSet.As<IQueryable<T>>().Setup(x => x.ElementType).Returns(queryable.ElementType);
        dbSet.As<IQueryable<T>>().Setup(x => x.GetEnumerator()).Returns(() => queryable.GetEnumerator());

        return dbSet.Object;
    }

    private sealed class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
    {
        private readonly IQueryProvider _inner;

        public TestAsyncQueryProvider(IQueryProvider inner)
        {
            _inner = inner;
        }

        public IQueryable CreateQuery(Expression expression)
            => new TestAsyncEnumerable<TEntity>(expression);

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
            => new TestAsyncEnumerable<TElement>(expression);

        public object? Execute(Expression expression)
            => _inner.Execute(expression);

        public TResult Execute<TResult>(Expression expression)
            => _inner.Execute<TResult>(expression);

        public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
        {
            var expectedResultType = typeof(TResult).GetGenericArguments()[0];
            var executionResult = typeof(IQueryProvider)
                .GetMethod(nameof(IQueryProvider.Execute), 1, new[] { typeof(Expression) })!
                .MakeGenericMethod(expectedResultType)
                .Invoke(_inner, new object[] { expression });

            return (TResult)typeof(Task)
                .GetMethod(nameof(Task.FromResult))!
                .MakeGenericMethod(expectedResultType)
                .Invoke(null, new[] { executionResult })!;
        }
    }

    private sealed class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
    {
        public TestAsyncEnumerable(IEnumerable<T> enumerable)
            : base(enumerable)
        {
        }

        public TestAsyncEnumerable(Expression expression)
            : base(expression)
        {
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            => new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());

        IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
    }

    private sealed class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _inner;

        public TestAsyncEnumerator(IEnumerator<T> inner)
        {
            _inner = inner;
        }

        public T Current => _inner.Current;

        public ValueTask DisposeAsync()
        {
            _inner.Dispose();
            return ValueTask.CompletedTask;
        }

        public ValueTask<bool> MoveNextAsync()
            => new(_inner.MoveNext());
    }
}
