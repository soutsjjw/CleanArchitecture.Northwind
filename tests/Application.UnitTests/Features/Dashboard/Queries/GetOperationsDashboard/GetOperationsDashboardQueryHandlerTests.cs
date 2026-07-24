using System.Collections;
using System.Linq.Expressions;
using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Features.Dashboard.Queries.GetOperationsDashboard;
using CleanArchitecture.Northwind.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace CleanArchitecture.Northwind.Application.UnitTests.Features.Dashboard.Queries.GetOperationsDashboard;

public class GetOperationsDashboardQueryHandlerTests
{
    [Test]
    public async Task HandleShouldTreatOrdersWithoutRequiredDateAsPendingAndIncludeStockAtReorderLevel()
    {
        var context = new Mock<IApplicationDbContext>();
        context.Setup(x => x.Orders).Returns(CreateDbSet(new[]
        {
            CreateOrder(1, "ALFKI", "Alfreds", new DateTime(2026, 7, 24), null, null, 10m, 1, 0),
            CreateOrder(2, "OLD", "Old", new DateTime(2026, 6, 30), null, new DateTime(2026, 7, 1), 999m, 1, 0)
        }));
        context.Setup(x => x.Products).Returns(CreateDbSet(new[]
        {
            new Product { Id = 1, ProductName = "At level", UnitsInStock = 5, ReorderLevel = 5 }
        }));

        var result = await new GetOperationsDashboardQueryHandler(context.Object, Mock.Of<IDateTimeService>(x => x.Now == new DateTime(2026, 7, 24)))
            .Handle(new GetOperationsDashboardQuery(), CancellationToken.None);

        result.Data.MonthlyOrderCount.ShouldBe(1);
        result.Data.ShippingStatuses.Single(x => x.Status == DashboardShippingStatus.Pending).Count.ShouldBe(1);
        result.Data.LowStockProducts.Single().ProductName.ShouldBe("At level");
    }

    [Test]
    public async Task HandleShouldBuildDashboardForCurrentDayAndMonth()
    {
        var context = new Mock<IApplicationDbContext>();
        context.Setup(x => x.Orders).Returns(CreateDbSet(new[]
        {
            CreateOrder(1, "ALFKI", "Alfreds", new DateTime(2026, 7, 24), null, new DateTime(2026, 7, 25), 100m, 1, 0),
            CreateOrder(2, "BONAP", "Bon app", new DateTime(2026, 7, 10), new DateTime(2026, 7, 12), new DateTime(2026, 7, 11), 50m, 2, 0.1f),
            CreateOrder(3, "ALFKI", "Alfreds", new DateTime(2026, 7, 1), null, new DateTime(2026, 7, 20), 40m, 1, 0),
            CreateOrder(4, "OLD", "Old", new DateTime(2026, 6, 30), null, new DateTime(2026, 7, 1), 999m, 1, 0),
            CreateOrder(5, "DELETE", "Deleted", new DateTime(2026, 7, 24), null, null, 999m, 1, 0, isDelete: true)
        }));
        context.Setup(x => x.Products).Returns(CreateDbSet(new[]
        {
            new Product { Id = 1, ProductName = "Out of stock", UnitsInStock = 0, ReorderLevel = 10 },
            new Product { Id = 2, ProductName = "Needs reorder", UnitsInStock = 3, ReorderLevel = 5 },
            new Product { Id = 3, ProductName = "Enough stock", UnitsInStock = 10, ReorderLevel = 5 },
            new Product { Id = 4, ProductName = "Discontinued", UnitsInStock = 0, ReorderLevel = 10, Discontinued = true }
        }));
        var clock = Mock.Of<IDateTimeService>(x => x.Now == new DateTime(2026, 7, 24, 10, 0, 0));

        var result = await new GetOperationsDashboardQueryHandler(context.Object, clock)
            .Handle(new GetOperationsDashboardQuery(), CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        result.Data.TodayOrderCount.ShouldBe(1);
        result.Data.TodayRevenue.ShouldBe(100m);
        result.Data.MonthlyOrderCount.ShouldBe(3);
        result.Data.MonthlyRevenue.ShouldBe(230m);
        result.Data.ShippingStatuses.ShouldContain(x => x.Status == DashboardShippingStatus.Shipped && x.Count == 1);
        result.Data.ShippingStatuses.ShouldContain(x => x.Status == DashboardShippingStatus.Pending && x.Count == 1);
        result.Data.ShippingStatuses.ShouldContain(x => x.Status == DashboardShippingStatus.Overdue && x.Count == 1);
        result.Data.LowStockProducts.Select(x => x.ProductName).ShouldBe(["Out of stock", "Needs reorder"]);
        result.Data.TopCustomers.Select(x => x.CompanyName).ShouldBe(["Alfreds", "Bon app"]);
    }

    private static Order CreateOrder(int id, string customerId, string companyName, DateTime orderDate, DateTime? shippedDate, DateTime? requiredDate, decimal unitPrice, short quantity, float discount, bool isDelete = false)
        => new()
        {
            Id = id,
            CustomerId = customerId,
            Customer = new Customer { Id = customerId, CompanyName = companyName },
            OrderDate = orderDate,
            ShippedDate = shippedDate,
            RequiredDate = requiredDate,
            IsDelete = isDelete,
            OrderDetails = { new OrderDetail { OrderId = id, ProductId = id, UnitPrice = unitPrice, Quantity = quantity, Discount = discount } }
        };

    private static DbSet<T> CreateDbSet<T>(IEnumerable<T> source) where T : class
    {
        var queryable = source.AsQueryable();
        var dbSet = new Mock<DbSet<T>>();
        dbSet.As<IAsyncEnumerable<T>>().Setup(x => x.GetAsyncEnumerator(It.IsAny<CancellationToken>())).Returns(new TestAsyncEnumerator<T>(queryable.GetEnumerator()));
        dbSet.As<IQueryable<T>>().Setup(x => x.Provider).Returns(new TestAsyncQueryProvider<T>(queryable.Provider));
        dbSet.As<IQueryable<T>>().Setup(x => x.Expression).Returns(queryable.Expression);
        dbSet.As<IQueryable<T>>().Setup(x => x.ElementType).Returns(queryable.ElementType);
        dbSet.As<IQueryable<T>>().Setup(x => x.GetEnumerator()).Returns(() => queryable.GetEnumerator());
        return dbSet.Object;
    }

    private sealed class TestAsyncQueryProvider<T>(IQueryProvider inner) : IAsyncQueryProvider
    {
        public IQueryable CreateQuery(Expression expression) => new TestAsyncEnumerable<T>(expression);
        public IQueryable<TElement> CreateQuery<TElement>(Expression expression) => new TestAsyncEnumerable<TElement>(expression);
        public object? Execute(Expression expression) => inner.Execute(expression);
        public TResult Execute<TResult>(Expression expression) => inner.Execute<TResult>(expression);
        public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
        {
            var resultType = typeof(TResult).GetGenericArguments()[0];
            var result = typeof(IQueryProvider).GetMethod(nameof(IQueryProvider.Execute), 1, [typeof(Expression)])!.MakeGenericMethod(resultType).Invoke(inner, [expression]);
            return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))!.MakeGenericMethod(resultType).Invoke(null, [result])!;
        }
    }

    private sealed class TestAsyncEnumerable<T>(Expression expression) : EnumerableQuery<T>(expression), IAsyncEnumerable<T>, IQueryable<T>
    {
        public TestAsyncEnumerable(IEnumerable<T> enumerable) : this(enumerable.AsQueryable().Expression) { }
        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) => new TestAsyncEnumerator<T>(((IEnumerable<T>)this).GetEnumerator());
        IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
    }

    private sealed class TestAsyncEnumerator<T>(IEnumerator<T> inner) : IAsyncEnumerator<T>
    {
        public T Current => inner.Current;
        public ValueTask DisposeAsync() { inner.Dispose(); return ValueTask.CompletedTask; }
        public ValueTask<bool> MoveNextAsync() => new(inner.MoveNext());
    }
}
