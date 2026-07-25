using System.Collections;
using System.Linq.Expressions;
using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Features.Categories.Commands.DeleteCategory;
using CleanArchitecture.Northwind.Application.Features.Categories.Commands.SetCategoryActive;
using CleanArchitecture.Northwind.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace CleanArchitecture.Northwind.Application.UnitTests.Features.Categories.Commands;

public class CategoryCommandHandlerTests
{
    [Test]
    public async Task DeleteCategoryShouldRejectInvalidIdBeforeQuerying()
    {
        var fixture = CreateFixture([]);
        var handler = new DeleteCategoryCommandHandler(fixture.Context.Object);

        var result = await handler.Handle(
            new DeleteCategoryCommand { Id = 0 },
            CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.StatusCode.ShouldBe(400);
        fixture.Context.Verify(
            value => value.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task SetCategoryActiveShouldRejectInvalidIdBeforeQuerying()
    {
        var fixture = CreateFixture([]);
        var handler = new SetCategoryActiveCommandHandler(fixture.Context.Object);

        var result = await handler.Handle(
            new SetCategoryActiveCommand { Id = -1, IsActive = true },
            CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.StatusCode.ShouldBe(400);
        fixture.Context.Verify(
            value => value.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task DeleteCategoryShouldRejectWhenProductsExist()
    {
        var category = new Category
        {
            Id = 3,
            CategoryName = "飲料",
            IsActive = true
        };
        var categories = CreateDbSet(new[] { category });
        var products = CreateDbSet(new[]
        {
            new Product
            {
                Id = 42,
                ProductName = "測試商品",
                CategoryId = category.Id,
                RowVersion = [1, 2, 3]
            }
        });
        var context = new Mock<IApplicationDbContext>();
        context.Setup(x => x.Categories).Returns(categories.Object);
        context.Setup(x => x.Products).Returns(products.Object);
        categories
            .Setup(x => x.FindAsync(new object[] { category.Id }, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
        var handler = new DeleteCategoryCommandHandler(context.Object);

        var result = await handler.Handle(
            new DeleteCategoryCommand { Id = category.Id },
            CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        category.IsDelete.ShouldBeFalse();
        context.Verify(
            value => value.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task SetCategoryActiveShouldPersistRequestedState(bool isActive)
    {
        var category = new Category
        {
            Id = 3,
            CategoryName = "飲料",
            IsActive = !isActive
        };
        var fixture = CreateFixture([category]);
        var handler = new SetCategoryActiveCommandHandler(fixture.Context.Object);

        var result = await handler.Handle(
            new SetCategoryActiveCommand
            {
                Id = category.Id,
                IsActive = isActive
            },
            CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        category.IsActive.ShouldBe(isActive);
        fixture.Context.Verify(
            value => value.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task DeleteCategoryShouldSoftDeleteWhenUnused()
    {
        var category = new Category
        {
            Id = 3,
            CategoryName = "飲料",
            IsActive = true
        };
        var fixture = CreateFixture([category]);
        var handler = new DeleteCategoryCommandHandler(fixture.Context.Object);

        var result = await handler.Handle(
            new DeleteCategoryCommand { Id = category.Id },
            CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        category.IsDelete.ShouldBeTrue();
        category.IsActive.ShouldBeFalse();
        fixture.Context.Verify(
            value => value.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static CategoryFixture CreateFixture(
        IReadOnlyList<Category> categories,
        IReadOnlyList<Product>? products = null)
    {
        products ??= [];
        var categorySet = CreateDbSet(categories);
        var productSet = CreateDbSet(products);
        var context = new Mock<IApplicationDbContext>();
        context.Setup(x => x.Categories).Returns(categorySet.Object);
        context.Setup(x => x.Products).Returns(productSet.Object);
        categorySet
            .Setup(x => x.FindAsync(
                It.IsAny<object[]>(),
                It.IsAny<CancellationToken>()))
            .Returns((object[] ids, CancellationToken _) =>
                new ValueTask<Category?>(
                    categories.SingleOrDefault(category => category.Id == (int)ids[0])));

        return new CategoryFixture(context);
    }

    private static Mock<DbSet<T>> CreateDbSet<T>(IEnumerable<T> source)
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

    private sealed record CategoryFixture(Mock<IApplicationDbContext> Context);
}
