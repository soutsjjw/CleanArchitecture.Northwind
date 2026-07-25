using System.Collections;
using System.Linq.Expressions;
using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Features.Products.Queries.GetProductFormOptions;
using CleanArchitecture.Northwind.Application.Features.Products.Queries.GetProducts;
using CleanArchitecture.Northwind.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace CleanArchitecture.Northwind.Application.UnitTests.Features.Products.Queries;

public class ProductQueryHandlerTests
{
    [Test]
    public async Task FormOptionsShouldReturnOnlyActiveNonDeletedValuesInNameOrder()
    {
        var categories = new[]
        {
            new Category { Id = 3, CategoryName = "B Category", IsActive = true },
            new Category { Id = 2, CategoryName = "A Category", IsActive = true },
            new Category { Id = 1, CategoryName = "停用", IsActive = false },
            new Category { Id = 4, CategoryName = "刪除", IsActive = true, IsDelete = true }
        };
        var suppliers = new[]
        {
            new Supplier { Id = 8, CompanyName = "B Supplier", IsActive = true },
            new Supplier { Id = 7, CompanyName = "A Supplier", IsActive = true },
            new Supplier { Id = 6, CompanyName = "停用供應商", IsActive = false },
            new Supplier
            {
                Id = 9,
                CompanyName = "刪除供應商",
                IsActive = true,
                IsDelete = true
            }
        };
        var context = new Mock<IApplicationDbContext>();
        context.Setup(value => value.Categories).Returns(CreateDbSet(categories).Object);
        context.Setup(value => value.Suppliers).Returns(CreateDbSet(suppliers).Object);
        var handler = new GetProductFormOptionsQueryHandler(context.Object);

        var result = await handler.Handle(
            new GetProductFormOptionsQuery(),
            CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        result.Data.Categories.Select(option => option.Id).ShouldBe([2, 3]);
        result.Data.Suppliers.Select(option => option.Id).ShouldBe([7, 8]);
    }

    [Test]
    public async Task ProductListShouldApplyLowStockAndCategoryFiltersThenSortAndPage()
    {
        var category = new Category
        {
            Id = 3,
            CategoryName = "飲料",
            IsActive = true
        };
        var supplier = new Supplier
        {
            Id = 5,
            CompanyName = "供應商",
            IsActive = true
        };
        var products = new[]
        {
            Product(1, "B 商品", category, supplier, stock: 2, reorderLevel: 2),
            Product(2, "A 商品", category, supplier, stock: 1, reorderLevel: 2),
            Product(3, "C 商品", category, supplier, stock: 5, reorderLevel: 2),
            Product(4, "停用商品", category, supplier, stock: 0, reorderLevel: 2, discontinued: true),
            Product(5, "刪除商品", category, supplier, stock: 0, reorderLevel: 2, isDelete: true)
        };
        var context = new Mock<IApplicationDbContext>();
        context.Setup(value => value.Products).Returns(CreateDbSet(products).Object);
        var handler = new GetProductsQueryHandler(context.Object);

        var result = await handler.Handle(
            new GetProductsQuery
            {
                CategoryId = category.Id,
                LowStockOnly = true,
                PageNumber = 2,
                PageSize = 1
            },
            CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        result.Data.TotalCount.ShouldBe(2);
        result.Data.PageNumber.ShouldBe(2);
        result.Data.Items.Select(item => item.ProductName).ShouldBe(["B 商品"]);
    }

    private static Product Product(
        int id,
        string name,
        Category category,
        Supplier supplier,
        short stock,
        short reorderLevel,
        bool discontinued = false,
        bool isDelete = false)
        => new()
        {
            Id = id,
            ProductName = name,
            CategoryId = category.Id,
            Category = category,
            SupplierId = supplier.Id,
            Supplier = supplier,
            UnitsInStock = stock,
            ReorderLevel = reorderLevel,
            Discontinued = discontinued,
            IsDelete = isDelete,
            RowVersion = [1]
        };

    private static Mock<DbSet<T>> CreateDbSet<T>(IEnumerable<T> source)
        where T : class
    {
        var queryable = source.AsQueryable();
        var dbSet = new Mock<DbSet<T>>();

        dbSet.As<IAsyncEnumerable<T>>()
            .Setup(value => value.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(new TestAsyncEnumerator<T>(queryable.GetEnumerator()));
        dbSet.As<IQueryable<T>>()
            .Setup(value => value.Provider)
            .Returns(new TestAsyncQueryProvider<T>(queryable.Provider));
        dbSet.As<IQueryable<T>>().Setup(value => value.Expression).Returns(queryable.Expression);
        dbSet.As<IQueryable<T>>().Setup(value => value.ElementType).Returns(queryable.ElementType);
        dbSet.As<IQueryable<T>>()
            .Setup(value => value.GetEnumerator())
            .Returns(() => queryable.GetEnumerator());

        return dbSet;
    }

    private sealed class TestAsyncQueryProvider<TEntity>(IQueryProvider inner)
        : IAsyncQueryProvider
    {
        public IQueryable CreateQuery(Expression expression)
            => new TestAsyncEnumerable<TEntity>(expression);

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
            => new TestAsyncEnumerable<TElement>(expression);

        public object? Execute(Expression expression)
            => inner.Execute(expression);

        public TResult Execute<TResult>(Expression expression)
            => inner.Execute<TResult>(expression);

        public TResult ExecuteAsync<TResult>(
            Expression expression,
            CancellationToken cancellationToken = default)
        {
            var resultType = typeof(TResult).GetGenericArguments()[0];
            var result = typeof(IQueryProvider)
                .GetMethod(nameof(IQueryProvider.Execute), 1, [typeof(Expression)])!
                .MakeGenericMethod(resultType)
                .Invoke(inner, [expression]);

            return (TResult)typeof(Task)
                .GetMethod(nameof(Task.FromResult))!
                .MakeGenericMethod(resultType)
                .Invoke(null, [result])!;
        }
    }

    private sealed class TestAsyncEnumerable<T>(Expression expression)
        : EnumerableQuery<T>(expression), IAsyncEnumerable<T>, IQueryable<T>
    {
        public IAsyncEnumerator<T> GetAsyncEnumerator(
            CancellationToken cancellationToken = default)
            => new TestAsyncEnumerator<T>(((IEnumerable<T>)this).GetEnumerator());

        IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
    }

    private sealed class TestAsyncEnumerator<T>(IEnumerator<T> inner)
        : IAsyncEnumerator<T>
    {
        public T Current => inner.Current;

        public ValueTask DisposeAsync()
        {
            inner.Dispose();
            return ValueTask.CompletedTask;
        }

        public ValueTask<bool> MoveNextAsync()
            => new(inner.MoveNext());
    }
}
