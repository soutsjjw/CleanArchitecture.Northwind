using CleanArchitecture.Northwind.Domain.Entities;
using CleanArchitecture.Northwind.Domain.Entities.Identity;
using CleanArchitecture.Northwind.Domain.Enums;
using CleanArchitecture.Northwind.Infrastructure.Data;
using CleanArchitecture.Northwind.Infrastructure.Data.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using NUnit.Framework;
using Shouldly;

namespace CleanArchitecture.Northwind.Application.UnitTests.Common;

public class InventoryModelTests
{
    [Test]
    public void InventoryModelConfigurationShouldEnforceConcurrencyAndTransactionConstraints()
    {
        using var context = CreateContext();

        var product = context.Model.FindEntityType(typeof(Product)).ShouldNotBeNull();
        var rowVersion = product.FindProperty(nameof(Product.RowVersion)).ShouldNotBeNull();
        rowVersion.IsConcurrencyToken.ShouldBeTrue();
        rowVersion.ValueGenerated.ShouldBe(ValueGenerated.OnAddOrUpdate);

        var transaction = context.Model.FindEntityType(typeof(InventoryTransaction)).ShouldNotBeNull();
        var reason = transaction.FindProperty(nameof(InventoryTransaction.Reason)).ShouldNotBeNull();
        reason.IsNullable.ShouldBeFalse();
        reason.GetMaxLength().ShouldBe(250);

        transaction.GetIndexes()
            .Single(index => index.Properties.Select(property => property.Name)
                .SequenceEqual(new[] { nameof(InventoryTransaction.ProductId), nameof(InventoryTransaction.Created) }))
            .ShouldNotBeNull();

        transaction.GetForeignKeys()
            .Single(foreignKey => foreignKey.PrincipalEntityType.ClrType == typeof(Product))
            .DeleteBehavior.ShouldBe(DeleteBehavior.Restrict);
    }

    [TestCase(typeof(Category))]
    [TestCase(typeof(Supplier))]
    public void ActiveEntityConfigurationShouldRequireAndDefaultIsActiveToTrue(Type entityType)
    {
        using var context = CreateContext();

        var property = context.Model.FindEntityType(entityType)
            .ShouldNotBeNull()
            .FindProperty("IsActive")
            .ShouldNotBeNull();

        property.IsNullable.ShouldBeFalse();
        property.GetDefaultValue().ShouldBe(true);
    }

    [Test]
    public void UpdateEntitiesShouldSetAuditFieldsForGenericAuditableEntities()
    {
        var timestamp = new DateTimeOffset(2026, 7, 25, 12, 0, 0, TimeSpan.Zero);
        var interceptor = new AuditableEntityInterceptor(new TestUser("inventory-user"), new FixedTimeProvider(timestamp));
        using var context = CreateContext();
        var transaction = new InventoryTransaction
        {
            ProductId = 1,
            Reason = "Initial stock"
        };

        context.Add(transaction);
        interceptor.UpdateEntities(context);

        transaction.Created.ShouldBe(timestamp);
        transaction.CreatedBy.ShouldBe("inventory-user");
    }

    [Test]
    public void UpdateEntitiesShouldSetDateTimeAuditFieldsForApplicationUserProfile()
    {
        var timestamp = new DateTimeOffset(2026, 7, 25, 12, 0, 0, TimeSpan.Zero);
        var interceptor = new AuditableEntityInterceptor(new TestUser("profile-user"), new FixedTimeProvider(timestamp));
        using var context = CreateContext();
        var profile = new ApplicationUserProfile
        {
            UserId = "profile-user"
        };

        var profileEntry = context.Add(profile);

        interceptor.UpdateEntities(context);
        profile.Created.ShouldBe(timestamp.UtcDateTime);
        profile.CreatedBy.ShouldBe("profile-user");
        profile.LastModified.ShouldBe(timestamp.UtcDateTime);
        profile.LastModifiedBy.ShouldBe("profile-user");

        profileEntry.Properties
            .Single(property => property.Metadata.Name == nameof(ApplicationUserProfile.Created) &&
                                property.Metadata.ClrType == typeof(DateTime?))
            .CurrentValue.ShouldBe(timestamp.UtcDateTime);
    }

    [Test]
    public void CleanupFailedInventoryUpdateShouldClearForcedProductUpdateAndPendingHistory()
    {
        using var context = CreateContext();
        var persistedVersion = new byte[] { 4, 5, 6 };
        var requestedVersion = new byte[] { 1, 2, 3 };
        var product = new Product
        {
            Id = 42,
            ProductName = "測試商品",
            UnitsInStock = 8,
            RowVersion = persistedVersion
        };
        var transaction = new InventoryTransaction
        {
            ProductId = product.Id,
            TransactionType = InventoryTransactionType.Stocktake,
            QuantityBefore = 8,
            QuantityDelta = 0,
            QuantityAfter = 8,
            Reason = "零差異盤點"
        };

        context.Attach(product);
        context.InventoryTransactions.Add(transaction);
        context.PrepareInventoryUpdate(product, requestedVersion);

        var productEntry = context.Entry(product);
        productEntry.Property(x => x.RowVersion).OriginalValue.ShouldBe(requestedVersion);
        productEntry.Property(x => x.UnitsInStock).IsModified.ShouldBeTrue();

        context.CleanupFailedInventoryUpdate(product, transaction);

        productEntry.State.ShouldBe(EntityState.Unchanged);
        productEntry.Property(x => x.UnitsInStock).IsModified.ShouldBeFalse();
        context.Entry(transaction).State.ShouldBe(EntityState.Detached);
        context.ChangeTracker.HasChanges().ShouldBeFalse();
    }

    private static ApplicationDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=InventoryModelTests;Integrated Security=True;TrustServerCertificate=True")
            .Options);

    private sealed class TestUser(string id) : Application.Common.Interfaces.IUser
    {
        public string? Id => id;

        public List<string>? Roles => null;
    }

    private sealed class FixedTimeProvider(DateTimeOffset timestamp) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => timestamp;
    }
}
