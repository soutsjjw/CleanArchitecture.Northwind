using System.Collections;
using System.Linq.Expressions;
using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Features.Products.Commands.CreateProduct;
using CleanArchitecture.Northwind.Application.Features.Products.Commands.DeleteProduct;
using CleanArchitecture.Northwind.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace CleanArchitecture.Northwind.Application.UnitTests.Features.Products.Commands;

public class ProductCommandHandlerTests
{
    [Test]
    public async Task DeleteProductShouldRejectWhenInventoryHistoryExists()
    {
        var product = CreateProduct();
        var fixture = CreateFixture(
            [product],
            [new InventoryTransaction { Id = 1, ProductId = product.Id }]);
        var handler = new DeleteProductCommandHandler(fixture.Context.Object);

        var result = await handler.Handle(
            new DeleteProductCommand { Id = product.Id },
            CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        product.IsDelete.ShouldBeFalse();
        fixture.Context.Verify(
            context => context.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [TestCase(false, true)]
    [TestCase(true, false)]
    public async Task CreateProductShouldRejectInactiveCategoryOrSupplier(
        bool categoryIsActive,
        bool supplierIsActive)
    {
        var category = new Category
        {
            Id = 3,
            CategoryName = "飲料",
            IsActive = categoryIsActive
        };
        var supplier = new Supplier
        {
            Id = 5,
            CompanyName = "供應商",
            IsActive = supplierIsActive
        };
        var fixture = CreateFixture([], [], [category], [supplier]);
        var handler = new CreateProductCommandHandler(fixture.Context.Object);

        var result = await handler.Handle(
            new CreateProductCommand
            {
                ProductName = "測試商品",
                CategoryId = category.Id,
                SupplierId = supplier.Id
            },
            CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        fixture.Products.Verify(
            products => products.Add(It.IsAny<Product>()),
            Times.Never);
        fixture.Context.Verify(
            context => context.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static Product CreateProduct()
        => new()
        {
            Id = 42,
            ProductName = "測試商品",
            RowVersion = [1, 2, 3]
        };

    private static ProductFixture CreateFixture(
        IReadOnlyList<Product> products,
        IReadOnlyList<InventoryTransaction> inventoryTransactions,
        IReadOnlyList<Category>? categories = null,
        IReadOnlyList<Supplier>? suppliers = null)
    {
        categories ??= [];
        suppliers ??= [];

        var productSet = CreateDbSet(products);
        var inventoryTransactionSet = CreateDbSet(inventoryTransactions);
        var categorySet = CreateDbSet(categories);
        var supplierSet = CreateDbSet(suppliers);
        var orderDetailSet = CreateDbSet(Array.Empty<OrderDetail>());
        var context = new Mock<IApplicationDbContext>();

        context.Setup(x => x.Products).Returns(productSet.Object);
        context.Setup(x => x.InventoryTransactions).Returns(inventoryTransactionSet.Object);
        context.Setup(x => x.Categories).Returns(categorySet.Object);
        context.Setup(x => x.Suppliers).Returns(supplierSet.Object);
        context.Setup(x => x.OrderDetails).Returns(orderDetailSet.Object);
        productSet
            .Setup(x => x.FindAsync(new object[] { 42 }, It.IsAny<CancellationToken>()))
            .ReturnsAsync(products.SingleOrDefault(product => product.Id == 42));

        return new ProductFixture(context, productSet);
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

    private sealed record ProductFixture(
        Mock<IApplicationDbContext> Context,
        Mock<DbSet<Product>> Products);
}
