using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Features.Inventory.Commands.AdjustInventory;
using CleanArchitecture.Northwind.Application.Features.Inventory.Commands.Stocktake;
using CleanArchitecture.Northwind.Domain.Entities;
using CleanArchitecture.Northwind.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace CleanArchitecture.Northwind.Application.UnitTests.Features.Inventory.Commands;

public class InventoryCommandHandlerTests
{
    private static readonly byte[] CurrentVersion = [1, 2, 3];

    [TestCase((short)-11)]
    [TestCase((short)-1)]
    public async Task AdjustShouldRejectNegativeResult(short delta)
    {
        var product = CreateProduct(unitsInStock: 0);
        var fixture = CreateFixture(product);
        var handler = new AdjustInventoryCommandHandler(fixture.Context.Object);

        var result = await handler.Handle(
            new AdjustInventoryCommand(product.Id, delta, "盤損", CurrentVersion),
            CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        fixture.Transactions.Verify(
            x => x.Add(It.IsAny<InventoryTransaction>()),
            Times.Never);
        fixture.Context.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task AdjustShouldRejectShortOverflow()
    {
        var product = CreateProduct(short.MaxValue);
        var fixture = CreateFixture(product);
        var handler = new AdjustInventoryCommandHandler(fixture.Context.Object);

        var result = await handler.Handle(
            new AdjustInventoryCommand(product.Id, 1, "調整", CurrentVersion),
            CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        product.UnitsInStock.ShouldBe(short.MaxValue);
        fixture.Transactions.Verify(
            x => x.Add(It.IsAny<InventoryTransaction>()),
            Times.Never);
        fixture.Context.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [TestCase(true, false)]
    [TestCase(false, true)]
    public async Task AdjustShouldRejectUnavailableProduct(bool discontinued, bool isDelete)
    {
        var product = CreateProduct(8);
        product.Discontinued = discontinued;
        product.IsDelete = isDelete;
        var fixture = CreateFixture(product);
        var handler = new AdjustInventoryCommandHandler(fixture.Context.Object);

        var result = await handler.Handle(
            new AdjustInventoryCommand(product.Id, 1, "調整", CurrentVersion),
            CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        fixture.Transactions.Verify(
            x => x.Add(It.IsAny<InventoryTransaction>()),
            Times.Never);
        fixture.Context.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [TestCase("")]
    [TestCase("   ")]
    public async Task AdjustShouldRejectEmptyReason(string reason)
    {
        var product = CreateProduct(8);
        var fixture = CreateFixture(product);
        var handler = new AdjustInventoryCommandHandler(fixture.Context.Object);

        var result = await handler.Handle(
            new AdjustInventoryCommand(product.Id, 1, reason, CurrentVersion),
            CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        fixture.Context.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task AdjustShouldRejectReasonLongerThanMaximum()
    {
        var product = CreateProduct(8);
        var fixture = CreateFixture(product);
        var handler = new AdjustInventoryCommandHandler(fixture.Context.Object);

        var result = await handler.Handle(
            new AdjustInventoryCommand(product.Id, 1, new string('原', 251), CurrentVersion),
            CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        fixture.Context.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task AdjustShouldRejectMissingRowVersion()
    {
        var product = CreateProduct(8);
        var fixture = CreateFixture(product);
        var handler = new AdjustInventoryCommandHandler(fixture.Context.Object);

        var result = await handler.Handle(
            new AdjustInventoryCommand(product.Id, 1, "調整", []),
            CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        fixture.Context.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task AdjustShouldRejectZeroDeltaWithoutSaving()
    {
        var product = CreateProduct(8);
        var fixture = CreateFixture(product);
        var handler = new AdjustInventoryCommandHandler(fixture.Context.Object);

        var result = await handler.Handle(
            new AdjustInventoryCommand(product.Id, 0, "沒有異動", CurrentVersion),
            CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        fixture.Context.Verify(
            x => x.PrepareInventoryUpdate(It.IsAny<Product>(), It.IsAny<byte[]>()),
            Times.Never);
        fixture.Transactions.Verify(
            x => x.Add(It.IsAny<InventoryTransaction>()),
            Times.Never);
        fixture.Context.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task AdjustShouldWriteManualAdjustmentAndReturnLatestState()
    {
        var product = CreateProduct(8);
        var latestVersion = new byte[] { 4, 5, 6 };
        var fixture = CreateFixture(product);
        fixture.Context
            .Setup(x => x.SaveChangesAsync(CancellationToken.None))
            .Callback(() => product.RowVersion = latestVersion)
            .ReturnsAsync(2);
        var handler = new AdjustInventoryCommandHandler(fixture.Context.Object);

        var result = await handler.Handle(
            new AdjustInventoryCommand(product.Id, 3, "  人工入庫  ", CurrentVersion),
            CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        result.Data.UnitsInStock.ShouldBe((short)11);
        result.Data.RowVersion.ShouldBe(latestVersion);
        product.UnitsInStock.ShouldBe((short)11);
        fixture.Context.Verify(
            x => x.PrepareInventoryUpdate(product, It.Is<byte[]>(version => version.SequenceEqual(CurrentVersion))),
            Times.Once);
        fixture.Transactions.Verify(
            x => x.Add(It.Is<InventoryTransaction>(transaction =>
                transaction.ProductId == product.Id &&
                transaction.TransactionType == InventoryTransactionType.ManualAdjustment &&
                transaction.QuantityBefore == 8 &&
                transaction.QuantityDelta == 3 &&
                transaction.QuantityAfter == 11 &&
                transaction.Reason == "人工入庫")),
            Times.Once);
        fixture.Context.Verify(
            x => x.SaveChangesAsync(CancellationToken.None),
            Times.Once);
    }

    [Test]
    public async Task StocktakeShouldWriteActualMinusCurrentAsDelta()
    {
        var product = CreateProduct(8);
        var latestVersion = new byte[] { 7, 8, 9 };
        var fixture = CreateFixture(product);
        fixture.Context
            .Setup(x => x.SaveChangesAsync(CancellationToken.None))
            .Callback(() => product.RowVersion = latestVersion)
            .ReturnsAsync(2);
        var handler = new StocktakeCommandHandler(fixture.Context.Object);

        var result = await handler.Handle(
            new StocktakeCommand(product.Id, 11, "循環盤點", CurrentVersion),
            CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        result.Data.UnitsInStock.ShouldBe((short)11);
        result.Data.RowVersion.ShouldBe(latestVersion);
        fixture.Transactions.Verify(
            x => x.Add(It.Is<InventoryTransaction>(transaction =>
                transaction.ProductId == product.Id &&
                transaction.TransactionType == InventoryTransactionType.Stocktake &&
                transaction.QuantityBefore == 8 &&
                transaction.QuantityDelta == 3 &&
                transaction.QuantityAfter == 11 &&
                transaction.Reason == "循環盤點")),
            Times.Once);
        fixture.Context.Verify(
            x => x.SaveChangesAsync(CancellationToken.None),
            Times.Once);
    }

    [Test]
    public async Task StocktakeShouldPrepareProductUpdateWhenActualEqualsCurrent()
    {
        var product = CreateProduct(8);
        var fixture = CreateFixture(product);
        var handler = new StocktakeCommandHandler(fixture.Context.Object);

        var result = await handler.Handle(
            new StocktakeCommand(product.Id, 8, "零差異盤點", CurrentVersion),
            CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        fixture.Context.Verify(
            x => x.PrepareInventoryUpdate(
                product,
                It.Is<byte[]>(version => version.SequenceEqual(CurrentVersion))),
            Times.Once);
        fixture.Transactions.Verify(
            x => x.Add(It.Is<InventoryTransaction>(transaction =>
                transaction.QuantityBefore == 8 &&
                transaction.QuantityDelta == 0 &&
                transaction.QuantityAfter == 8)),
            Times.Once);
        fixture.Context.Verify(
            x => x.SaveChangesAsync(CancellationToken.None),
            Times.Once);
    }

    [Test]
    public async Task StocktakeShouldRejectNegativeActualQuantity()
    {
        var product = CreateProduct(8);
        var fixture = CreateFixture(product);
        var handler = new StocktakeCommandHandler(fixture.Context.Object);

        var result = await handler.Handle(
            new StocktakeCommand(product.Id, -1, "循環盤點", CurrentVersion),
            CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        fixture.Transactions.Verify(
            x => x.Add(It.IsAny<InventoryTransaction>()),
            Times.Never);
        fixture.Context.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task AdjustShouldReturnConflictWhenRowVersionHasChanged()
    {
        var product = CreateProduct(8);
        var fixture = CreateFixture(product);
        fixture.Context
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DbUpdateConcurrencyException());
        var handler = new AdjustInventoryCommandHandler(fixture.Context.Object);

        var result = await handler.Handle(
            new AdjustInventoryCommand(product.Id, 3, "人工入庫", CurrentVersion),
            CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain("庫存已被其他使用者更新");
        product.UnitsInStock.ShouldBe((short)8);
        fixture.Context.Verify(
            x => x.CleanupFailedInventoryUpdate(
                product,
                It.Is<InventoryTransaction>(transaction =>
                    transaction.ProductId == product.Id &&
                    transaction.QuantityBefore == 8 &&
                    transaction.QuantityAfter == 11)),
            Times.Once);
    }

    [Test]
    public void AdjustValidatorShouldRejectInvalidBoundaryValues()
    {
        var validator = new AdjustInventoryCommandValidator();
        var command = new AdjustInventoryCommand(
            0,
            1,
            new string('原', 251),
            []);

        var result = validator.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.Select(error => error.PropertyName).ShouldContain(nameof(command.ProductId));
        result.Errors.Select(error => error.PropertyName).ShouldContain(nameof(command.Reason));
        result.Errors.Select(error => error.PropertyName).ShouldContain(nameof(command.RowVersion));
    }

    [Test]
    public void AdjustValidatorShouldRejectZeroDelta()
    {
        var validator = new AdjustInventoryCommandValidator();
        var command = new AdjustInventoryCommand(
            1,
            0,
            "沒有異動",
            CurrentVersion);

        var result = validator.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.Select(error => error.PropertyName)
            .ShouldContain(nameof(command.QuantityDelta));
    }

    [Test]
    public void StocktakeValidatorShouldRejectNegativeActualQuantityAndWhitespaceReason()
    {
        var validator = new StocktakeCommandValidator();
        var command = new StocktakeCommand(
            1,
            -1,
            "   ",
            CurrentVersion);

        var result = validator.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.Select(error => error.PropertyName).ShouldContain(nameof(command.ActualQuantity));
        result.Errors.Select(error => error.PropertyName).ShouldContain(nameof(command.Reason));
    }

    private static Product CreateProduct(short? unitsInStock)
    {
        return new Product
        {
            Id = 42,
            ProductName = "測試商品",
            UnitsInStock = unitsInStock,
            RowVersion = CurrentVersion.ToArray()
        };
    }

    private static InventoryFixture CreateFixture(Product product)
    {
        var products = new Mock<DbSet<Product>>();
        var transactions = new Mock<DbSet<InventoryTransaction>>();
        var context = new Mock<IApplicationDbContext>();
        context.Setup(x => x.Products).Returns(products.Object);
        context.Setup(x => x.InventoryTransactions).Returns(transactions.Object);
        context
            .Setup(x => x.CleanupFailedInventoryUpdate(
                It.IsAny<Product>(),
                It.IsAny<InventoryTransaction>()))
            .Callback<Product, InventoryTransaction>(
                (failedProduct, transaction) =>
                    failedProduct.UnitsInStock = transaction.QuantityBefore);
        products
            .Setup(x => x.FindAsync(new object[] { product.Id }, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        return new InventoryFixture(context, transactions);
    }

    private sealed record InventoryFixture(
        Mock<IApplicationDbContext> Context,
        Mock<DbSet<InventoryTransaction>> Transactions);
}
