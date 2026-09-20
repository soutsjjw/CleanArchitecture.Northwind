using System.Collections;
using System.Linq.Expressions;
using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Features.Suppliers.Commands.CreateSupplier;
using CleanArchitecture.Northwind.Application.Features.Suppliers.Commands.DeleteSupplier;
using CleanArchitecture.Northwind.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace CleanArchitecture.Northwind.Application.UnitTests.Features.Suppliers.Commands;

public class SupplierCommandHandlerTests
{
    [Test]
    public async Task CreateSupplierShouldRejectCaseInsensitiveDuplicateCompanyName()
    {
        var context = SupplierCommandTestFixture.CreateContext(
            [new Supplier { Id = 1, CompanyName = "Alpha Co", IsActive = true }], []);
        var handler = new CreateSupplierCommandHandler(context.Object);

        var result = await handler.Handle(
            new CreateSupplierCommand { CompanyName = " alpha co " },
            CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.StatusCode.ShouldBe(409);
    }

    [Test]
    public async Task CreateSupplierShouldAllowCompanyNameReusedAfterSoftDelete()
    {
        var context = SupplierCommandTestFixture.CreateContext(
            [new Supplier { Id = 1, CompanyName = "Alpha Co", IsDelete = true }], []);
        var handler = new CreateSupplierCommandHandler(context.Object);

        var result = await handler.Handle(
            new CreateSupplierCommand { CompanyName = "Alpha Co" },
            CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
    }

    [TestCase("ftp://example.test")]
    [TestCase("/supplier")]
    [TestCase("javascript:alert(1)")]
    public async Task CreateSupplierShouldRejectNonHttpHomePage(string homePage)
    {
        var context = SupplierCommandTestFixture.CreateContext([], []);
        var handler = new CreateSupplierCommandHandler(context.Object);

        var result = await handler.Handle(
            new CreateSupplierCommand { CompanyName = "Alpha Co", HomePage = homePage },
            CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.StatusCode.ShouldBe(400);
    }

    [TestCase("http://example.test")]
    [TestCase("https://example.test")]
    public async Task CreateSupplierShouldAcceptHttpHomePage(string homePage)
    {
        var context = SupplierCommandTestFixture.CreateContext([], []);
        var handler = new CreateSupplierCommandHandler(context.Object);

        var result = await handler.Handle(
            new CreateSupplierCommand { CompanyName = "Alpha Co", HomePage = homePage },
            CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
    }

    [Test]
    public async Task DeleteSupplierShouldRejectWhenAnyProductReferencesIt()
    {
        var supplier = new Supplier { Id = 3, CompanyName = "Alpha Co", IsActive = true };
        var context = SupplierCommandTestFixture.CreateContext(
            [supplier],
            [new Product { Id = 4, ProductName = "Retired product", SupplierId = supplier.Id, IsDelete = true, RowVersion = [1] }]);
        var handler = new DeleteSupplierCommandHandler(context.Object);

        var result = await handler.Handle(new DeleteSupplierCommand { Id = supplier.Id }, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.StatusCode.ShouldBe(409);
        supplier.IsDelete.ShouldBeFalse();
        supplier.IsActive.ShouldBeTrue();
    }
}

internal static class SupplierCommandTestFixture
{
    internal static Mock<IApplicationDbContext> CreateContext(
        IReadOnlyList<Supplier> suppliers,
        IReadOnlyList<Product> products)
    {
        var supplierSet = CreateDbSet(suppliers);
        var productSet = CreateDbSet(products);
        supplierSet.Setup(value => value.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .Returns((object[] ids, CancellationToken _) => new ValueTask<Supplier?>(suppliers.SingleOrDefault(value => value.Id == (int)ids[0])));
        var context = new Mock<IApplicationDbContext>();
        context.Setup(value => value.Suppliers).Returns(supplierSet.Object);
        context.Setup(value => value.Products).Returns(productSet.Object);
        return context;
    }

    private static Mock<DbSet<T>> CreateDbSet<T>(IEnumerable<T> source) where T : class
    {
        var queryable = source.AsQueryable();
        var dbSet = new Mock<DbSet<T>>();
        dbSet.As<IAsyncEnumerable<T>>().Setup(value => value.GetAsyncEnumerator(It.IsAny<CancellationToken>())).Returns(new AsyncEnumerator<T>(queryable.GetEnumerator()));
        dbSet.As<IQueryable<T>>().Setup(value => value.Provider).Returns(new AsyncQueryProvider<T>(queryable.Provider));
        dbSet.As<IQueryable<T>>().Setup(value => value.Expression).Returns(queryable.Expression);
        dbSet.As<IQueryable<T>>().Setup(value => value.ElementType).Returns(queryable.ElementType);
        dbSet.As<IQueryable<T>>().Setup(value => value.GetEnumerator()).Returns(() => queryable.GetEnumerator());
        return dbSet;
    }

    private sealed class AsyncQueryProvider<T>(IQueryProvider inner) : IAsyncQueryProvider
    {
        public IQueryable CreateQuery(Expression expression) => new AsyncEnumerable<T>(expression);
        public IQueryable<TElement> CreateQuery<TElement>(Expression expression) => new AsyncEnumerable<TElement>(expression);
        public object? Execute(Expression expression) => inner.Execute(expression);
        public TResult Execute<TResult>(Expression expression) => inner.Execute<TResult>(expression);
        public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
        {
            var type = typeof(TResult).GetGenericArguments()[0];
            var result = typeof(IQueryProvider).GetMethod(nameof(IQueryProvider.Execute), 1, [typeof(Expression)])!.MakeGenericMethod(type).Invoke(inner, [expression]);
            return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))!.MakeGenericMethod(type).Invoke(null, [result])!;
        }
    }

    private sealed class AsyncEnumerable<T>(Expression expression) : EnumerableQuery<T>(expression), IAsyncEnumerable<T>, IQueryable<T>
    {
        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) => new AsyncEnumerator<T>(((IEnumerable<T>)this).GetEnumerator());
        IQueryProvider IQueryable.Provider => new AsyncQueryProvider<T>(this);
    }

    private sealed class AsyncEnumerator<T>(IEnumerator<T> inner) : IAsyncEnumerator<T>
    {
        public T Current => inner.Current;
        public ValueTask DisposeAsync() { inner.Dispose(); return ValueTask.CompletedTask; }
        public ValueTask<bool> MoveNextAsync() => new(inner.MoveNext());
    }
}
