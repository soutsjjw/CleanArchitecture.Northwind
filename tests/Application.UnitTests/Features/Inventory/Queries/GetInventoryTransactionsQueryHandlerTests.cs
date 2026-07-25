using System.Collections;
using System.Linq.Expressions;
using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Features.Inventory.Queries.GetInventoryTransactions;
using CleanArchitecture.Northwind.Application.Features.Products.Queries.GetProductOptions;
using CleanArchitecture.Northwind.Domain.Entities;
using CleanArchitecture.Northwind.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace CleanArchitecture.Northwind.Application.UnitTests.Features.Inventory.Queries;

public class GetInventoryTransactionsQueryHandlerTests
{
    [Test]
    public async Task HandleShouldFilterByProductAndReturnNewestProjectedTransactionsFirst()
    {
        var sharedCreated = new DateTimeOffset(2026, 7, 25, 9, 0, 0, TimeSpan.Zero);
        var context = new Mock<IApplicationDbContext>();
        context.Setup(x => x.InventoryTransactions).Returns(CreateDbSet(new[]
        {
            CreateTransaction(10, 7, sharedCreated.AddMinutes(-1), 3, 2, 5, "older"),
            CreateTransaction(11, 7, sharedCreated, 5, -1, 4, "newer-lower-id"),
            CreateTransaction(12, 7, sharedCreated, 4, 4, 8, "newer-higher-id"),
            CreateTransaction(13, 8, sharedCreated.AddHours(1), 1, 1, 2, "other product")
        }));

        var result = await new GetInventoryTransactionsQueryHandler(context.Object)
            .Handle(new GetInventoryTransactionsQuery
            {
                ProductId = 7,
                PageNumber = 1,
                PageSize = 10
            }, CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        result.Data.TotalCount.ShouldBe(3);
        result.Data.Items.Select(x => x.Id).ShouldBe([12, 11, 10]);

        var newest = result.Data.Items[0];
        newest.TransactionType.ShouldBe(InventoryTransactionType.ManualAdjustment);
        newest.QuantityBefore.ShouldBe((short)4);
        newest.QuantityDelta.ShouldBe((short)4);
        newest.QuantityAfter.ShouldBe((short)8);
        newest.Reason.ShouldBe("newer-higher-id");
        newest.Created.ShouldBe(sharedCreated);
        newest.CreatedBy.ShouldBe("inventory-user");
    }

    [Test]
    public async Task HandleShouldPaginateInventoryTransactions()
    {
        var context = new Mock<IApplicationDbContext>();
        context.Setup(x => x.InventoryTransactions).Returns(CreateDbSet(new[]
        {
            CreateTransaction(1, 7, new DateTimeOffset(2026, 7, 25, 8, 0, 0, TimeSpan.Zero), 0, 1, 1, "first"),
            CreateTransaction(2, 7, new DateTimeOffset(2026, 7, 25, 9, 0, 0, TimeSpan.Zero), 1, 1, 2, "second"),
            CreateTransaction(3, 7, new DateTimeOffset(2026, 7, 25, 10, 0, 0, TimeSpan.Zero), 2, 1, 3, "third")
        }));

        var result = await new GetInventoryTransactionsQueryHandler(context.Object)
            .Handle(new GetInventoryTransactionsQuery
            {
                ProductId = 7,
                PageNumber = 2,
                PageSize = 2
            }, CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        result.Data.TotalCount.ShouldBe(3);
        result.Data.PageNumber.ShouldBe(2);
        result.Data.Items.Select(x => x.Id).ShouldBe([1]);
    }

    [Test]
    public async Task ProductOptionsShouldExcludeDeletedAndDiscontinuedProducts()
    {
        var context = new Mock<IApplicationDbContext>();
        context.Setup(x => x.Products).Returns(CreateDbSet(new[]
        {
            new Product { Id = 1, ProductName = "Zebra", IsDelete = false, Discontinued = false },
            new Product { Id = 2, ProductName = "Deleted", IsDelete = true, Discontinued = false },
            new Product { Id = 3, ProductName = "Discontinued", IsDelete = false, Discontinued = true },
            new Product { Id = 4, ProductName = "Alpha", IsDelete = false, Discontinued = false }
        }));

        var result = await new GetProductOptionsQueryHandler(context.Object)
            .Handle(new GetProductOptionsQuery(), CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        result.Data.Select(x => x.Id).ShouldBe([4, 1]);
        result.Data.Select(x => x.Name).ShouldBe(["Alpha", "Zebra"]);
    }

    private static InventoryTransaction CreateTransaction(
        int id,
        int productId,
        DateTimeOffset created,
        short quantityBefore,
        short quantityDelta,
        short quantityAfter,
        string reason)
        => new()
        {
            Id = id,
            ProductId = productId,
            TransactionType = InventoryTransactionType.ManualAdjustment,
            QuantityBefore = quantityBefore,
            QuantityDelta = quantityDelta,
            QuantityAfter = quantityAfter,
            Reason = reason,
            Created = created,
            CreatedBy = "inventory-user"
        };

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

    private sealed class TestAsyncQueryProvider<TEntity>(IQueryProvider inner) : IAsyncQueryProvider
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
        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            => new TestAsyncEnumerator<T>(((IEnumerable<T>)this).GetEnumerator());

        IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
    }

    private sealed class TestAsyncEnumerator<T>(IEnumerator<T> inner) : IAsyncEnumerator<T>
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
