using CleanArchitecture.Northwind.Domain.Entities;
using NUnit.Framework;
using Shouldly;

namespace CleanArchitecture.Northwind.Application.UnitTests.Common;

public class InventoryModelTests
{
    [Test]
    public void ProductConfigurationShouldUseRowVersionAndInventoryTransactionShouldRequireReason()
    {
        typeof(Product).GetProperty("RowVersion").ShouldNotBeNull();

        var inventoryTransactionType = typeof(Product).Assembly.GetType(
            "CleanArchitecture.Northwind.Domain.Entities.InventoryTransaction");

        inventoryTransactionType.ShouldNotBeNull();
        inventoryTransactionType!.GetProperty("Reason").ShouldNotBeNull();
    }
}
