using System.Collections;
using System.Linq.Expressions;
using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Features.Suppliers.Queries.GetSuppliers;
using CleanArchitecture.Northwind.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace CleanArchitecture.Northwind.Application.UnitTests.Features.Suppliers.Queries;

public class SupplierQueryHandlerTests
{
    [Test]
    public async Task SupplierListShouldFilterKeywordAndStateThenProjectProductCount()
    {
        var active = new Supplier { Id = 2, CompanyName = "Alpha Co", ContactName = "Amy", Phone = "01", Country = "Taiwan", IsActive = true };
        var suppliers = new[]
        {
            new Supplier { Id = 1, CompanyName = "Beta Co", IsActive = true }, active,
            new Supplier { Id = 3, CompanyName = "Alpha Disabled", IsActive = false },
            new Supplier { Id = 4, CompanyName = "Alpha Deleted", IsActive = true, IsDelete = true }
        };
        var products = new[]
        {
            new Product { Id = 10, ProductName = "A", SupplierId = active.Id, RowVersion = [1] },
            new Product { Id = 11, ProductName = "Deleted", SupplierId = active.Id, IsDelete = true, RowVersion = [1] }
        };
        var context = new Mock<IApplicationDbContext>();
        context.Setup(value => value.Suppliers).Returns(CreateDbSet(suppliers).Object);
        context.Setup(value => value.Products).Returns(CreateDbSet(products).Object);
        var handler = new GetSuppliersQueryHandler(context.Object);

        var result = await handler.Handle(new GetSuppliersQuery { Keyword = "Alpha", IsActive = true, PageSize = 10 }, CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        result.Data.Items.ShouldBe([new SupplierListItemDto(2, "Alpha Co", "Amy", "01", "Taiwan", true, 1)]);
    }

    [Test]
    public async Task SupplierListShouldSortByCountryDescending()
    {
        var suppliers = new[]
        {
            new Supplier { Id = 1, CompanyName = "Alpha", Country = "Taiwan", IsActive = true },
            new Supplier { Id = 2, CompanyName = "Beta", Country = "Japan", IsActive = true },
            new Supplier { Id = 3, CompanyName = "Gamma", Country = "Korea", IsActive = true }
        };
        var context = new Mock<IApplicationDbContext>();
        context.Setup(value => value.Suppliers).Returns(CreateDbSet(suppliers).Object);
        context.Setup(value => value.Products).Returns(CreateDbSet(Array.Empty<Product>()).Object);
        var handler = new GetSuppliersQueryHandler(context.Object);

        var result = await handler.Handle(new GetSuppliersQuery
        {
            SortBy = SupplierSortField.Country,
            SortDescending = true,
            PageSize = 10
        }, CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        result.Data.Items.Select(supplier => supplier.CompanyName).ShouldBe(["Alpha", "Gamma", "Beta"]);
    }

    [Test]
    public async Task SupplierListShouldSortByStatusAscending()
    {
        var suppliers = new[]
        {
            new Supplier { Id = 1, CompanyName = "Enabled", IsActive = true },
            new Supplier { Id = 2, CompanyName = "Disabled", IsActive = false }
        };
        var context = new Mock<IApplicationDbContext>();
        context.Setup(value => value.Suppliers).Returns(CreateDbSet(suppliers).Object);
        context.Setup(value => value.Products).Returns(CreateDbSet(Array.Empty<Product>()).Object);
        var handler = new GetSuppliersQueryHandler(context.Object);

        var result = await handler.Handle(new GetSuppliersQuery
        {
            SortBy = SupplierSortField.IsActive,
            PageSize = 10
        }, CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        result.Data.Items.Select(supplier => supplier.CompanyName).ShouldBe(["Disabled", "Enabled"]);
    }

    private static Mock<DbSet<T>> CreateDbSet<T>(IEnumerable<T> source) where T : class
    {
        var queryable = source.AsQueryable();
        var dbSet = new Mock<DbSet<T>>();
        dbSet.As<IAsyncEnumerable<T>>().Setup(value => value.GetAsyncEnumerator(It.IsAny<CancellationToken>())).Returns(new TestAsyncEnumerator<T>(queryable.GetEnumerator()));
        dbSet.As<IQueryable<T>>().Setup(value => value.Provider).Returns(new TestAsyncQueryProvider<T>(queryable.Provider));
        dbSet.As<IQueryable<T>>().Setup(value => value.Expression).Returns(queryable.Expression);
        dbSet.As<IQueryable<T>>().Setup(value => value.ElementType).Returns(queryable.ElementType);
        dbSet.As<IQueryable<T>>().Setup(value => value.GetEnumerator()).Returns(() => queryable.GetEnumerator());
        return dbSet;
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
            var result = typeof(IQueryProvider).GetMethod(nameof(IQueryProvider.Execute), 1, [typeof(Expression)])!.MakeGenericMethod(resultType).Invoke(inner, [expression]);
            return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))!.MakeGenericMethod(resultType).Invoke(null, [result])!;
        }
    }

    private sealed class TestAsyncEnumerable<T>(Expression expression) : EnumerableQuery<T>(expression), IAsyncEnumerable<T>, IQueryable<T>
    {
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
