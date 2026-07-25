using System.Collections;
using System.Linq.Expressions;
using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Features.Products.Commands.CreateProduct;
using CleanArchitecture.Northwind.Application.Features.Products.Commands.DeleteProduct;
using CleanArchitecture.Northwind.Application.Features.Products.Commands.SetProductDiscontinued;
using CleanArchitecture.Northwind.Application.Features.Products.Commands.UpdateProduct;
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
    public async Task DeleteProductShouldRejectInvalidIdBeforeQuerying()
    {
        var fixture = CreateFixture([], []);
        var handler = new DeleteProductCommandHandler(fixture.Context.Object);

        var result = await handler.Handle(
            new DeleteProductCommand { Id = 0 },
            CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.StatusCode.ShouldBe(400);
        fixture.Context.Verify(
            context => context.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task SetProductDiscontinuedShouldRejectInvalidIdBeforeQuerying()
    {
        var fixture = CreateFixture([], []);
        var handler = new SetProductDiscontinuedCommandHandler(fixture.Context.Object);

        var result = await handler.Handle(
            new SetProductDiscontinuedCommand { Id = -1, Discontinued = true },
            CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.StatusCode.ShouldBe(400);
        fixture.Context.Verify(
            context => context.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

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

    [Test]
    public async Task DeleteProductShouldRejectWhenOrderHistoryExists()
    {
        var product = CreateProduct();
        var fixture = CreateFixture(
            [product],
            [],
            orderDetails: [new OrderDetail { OrderId = 7, ProductId = product.Id }]);
        var handler = new DeleteProductCommandHandler(fixture.Context.Object);

        var result = await handler.Handle(
            new DeleteProductCommand { Id = product.Id },
            CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.StatusCode.ShouldBe(409);
        product.IsDelete.ShouldBeFalse();
        fixture.Context.Verify(
            context => context.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task DeleteProductShouldSoftDeleteWithoutClearingReferences()
    {
        var product = CreateProduct();
        product.CategoryId = 3;
        product.SupplierId = 5;
        var fixture = CreateFixture([product], []);
        var handler = new DeleteProductCommandHandler(fixture.Context.Object);

        var result = await handler.Handle(
            new DeleteProductCommand { Id = product.Id },
            CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        product.IsDelete.ShouldBeTrue();
        product.Discontinued.ShouldBeTrue();
        product.CategoryId.ShouldBe(3);
        product.SupplierId.ShouldBe(5);
        fixture.Context.Verify(
            context => context.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task SetProductDiscontinuedShouldPersistRequestedState(bool discontinued)
    {
        var product = CreateProduct();
        product.Discontinued = !discontinued;
        var fixture = CreateFixture([product], []);
        var handler = new SetProductDiscontinuedCommandHandler(fixture.Context.Object);

        var result = await handler.Handle(
            new SetProductDiscontinuedCommand
            {
                Id = product.Id,
                Discontinued = discontinued
            },
            CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        product.Discontinued.ShouldBe(discontinued);
        fixture.Context.Verify(
            context => context.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
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

    [TestCase(false, true)]
    [TestCase(true, false)]
    public async Task UpdateProductShouldRejectInactiveCategoryOrSupplier(
        bool categoryIsActive,
        bool supplierIsActive)
    {
        var product = CreateProduct();
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
        var fixture = CreateFixture([product], [], [category], [supplier]);
        var handler = new UpdateProductCommandHandler(fixture.Context.Object);

        var result = await handler.Handle(
            ValidUpdate(product.Id, category.Id, supplier.Id),
            CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.StatusCode.ShouldBe(400);
        product.CategoryId.ShouldNotBe(category.Id);
        product.SupplierId.ShouldNotBe(supplier.Id);
        fixture.Context.Verify(
            context => context.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task UpdateProductShouldPreserveInventoryAndExistingPicture()
    {
        var product = CreateProduct();
        product.UnitsInStock = 17;
        product.UnitsOnOrder = 4;
        product.Picture = [1, 2, 3];
        product.PictureContentType = "image/png";
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
        var fixture = CreateFixture([product], [], [category], [supplier]);
        var handler = new UpdateProductCommandHandler(fixture.Context.Object);

        var result = await handler.Handle(
            ValidUpdate(product.Id, category.Id, supplier.Id),
            CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        product.UnitsInStock.ShouldBe((short)17);
        product.UnitsOnOrder.ShouldBe((short)4);
        product.Picture.ShouldBe([1, 2, 3]);
        product.PictureContentType.ShouldBe("image/png");
    }

    [Test]
    public async Task UpdateProductShouldRemovePictureWhenRequested()
    {
        var product = CreateProduct();
        product.Picture = [1, 2, 3];
        product.PictureContentType = "image/png";
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
        var fixture = CreateFixture([product], [], [category], [supplier]);
        var handler = new UpdateProductCommandHandler(fixture.Context.Object);

        var result = await handler.Handle(
            ValidUpdate(product.Id, category.Id, supplier.Id) with
            {
                RemovePicture = true
            },
            CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        product.Picture.ShouldBeNull();
        product.PictureContentType.ShouldBeNull();
    }

    private static UpdateProductCommand ValidUpdate(
        int productId,
        int categoryId,
        int supplierId)
        => new()
        {
            Id = productId,
            ProductName = "更新商品",
            CategoryId = categoryId,
            SupplierId = supplierId,
            UnitPrice = 20,
            ReorderLevel = 3
        };

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
        IReadOnlyList<Supplier>? suppliers = null,
        IReadOnlyList<OrderDetail>? orderDetails = null)
    {
        categories ??= [];
        suppliers ??= [];
        orderDetails ??= [];

        var productSet = CreateDbSet(products);
        var inventoryTransactionSet = CreateDbSet(inventoryTransactions);
        var categorySet = CreateDbSet(categories);
        var supplierSet = CreateDbSet(suppliers);
        var orderDetailSet = CreateDbSet(orderDetails);
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
