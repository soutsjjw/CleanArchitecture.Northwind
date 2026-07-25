using CleanArchitecture.Northwind.Domain.Entities;
using CleanArchitecture.Northwind.Domain.Enums;
using CleanArchitecture.Northwind.Infrastructure.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Testcontainers.MsSql;

namespace CleanArchitecture.Northwind.InventoryMigrationTests;

public class InitialInventoryMigrationTests
{
    private const string PreviousMigration = "20250818154756_AddLastPasswordChangedDateColumn";
    private const string MigrationName = "AddProductInventoryManagement";

    [Test]
    public async Task MigrationShouldCreateOneOpeningBalanceWithoutChangingProductStock()
    {
        await using var database = new MsSqlBuilder().Build();
        await database.StartAsync();

        var connectionString = CreateIsolatedDatabaseConnectionString(database.GetConnectionString());

        try
        {
            await CreateDatabaseAsync(connectionString);
            int productId;
            int nullStockProductId;

            await using (var context = CreateContext(connectionString))
            {
                var migrator = context.GetService<IMigrator>();
                context.Database.GetMigrations().ShouldContain(migration => migration.EndsWith(MigrationName));

                await migrator.MigrateAsync(PreviousMigration);

                await context.Database.OpenConnectionAsync();
                await using (var command = context.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = """
                        INSERT INTO [Products] ([ProductName], [UnitsInStock], [Discontinued], [IsDelete], [Created], [CreatedBy])
                        VALUES (N'Existing product', 17, 0, 0, SYSUTCDATETIME(), N'test:setup');
                        SELECT CAST(SCOPE_IDENTITY() AS int);
                        """;
                    productId = Convert.ToInt32(await command.ExecuteScalarAsync());

                    command.CommandText = """
                        INSERT INTO [Products] ([ProductName], [UnitsInStock], [Discontinued], [IsDelete], [Created], [CreatedBy])
                        VALUES (N'Product without stock', NULL, 0, 0, SYSUTCDATETIME(), N'test:setup');
                        SELECT CAST(SCOPE_IDENTITY() AS int);
                        """;
                    nullStockProductId = Convert.ToInt32(await command.ExecuteScalarAsync());
                }
                await context.Database.CloseConnectionAsync();

                await migrator.MigrateAsync();
            }

            await using (var context = CreateContext(connectionString))
            {
                var product = await context.Products.SingleAsync(x => x.Id == productId);
                var transaction = await context.InventoryTransactions.SingleAsync(x => x.ProductId == product.Id);

                transaction.TransactionType.ShouldBe(InventoryTransactionType.OpeningBalance);
                transaction.QuantityBefore.ShouldBe(product.UnitsInStock!.Value);
                transaction.QuantityAfter.ShouldBe(product.UnitsInStock.Value);
                transaction.QuantityDelta.ShouldBe((short)0);
                transaction.CreatedBy.ShouldBe("system:initial-balance");

                var nullStockTransaction = await context.InventoryTransactions.SingleAsync(x => x.ProductId == nullStockProductId);
                nullStockTransaction.TransactionType.ShouldBe(InventoryTransactionType.OpeningBalance);
                nullStockTransaction.QuantityBefore.ShouldBe((short)0);
                nullStockTransaction.QuantityAfter.ShouldBe((short)0);
                nullStockTransaction.QuantityDelta.ShouldBe((short)0);

                await context.Database.MigrateAsync();
                (await context.InventoryTransactions.CountAsync(x => x.ProductId == product.Id)).ShouldBe(1);
                (await context.InventoryTransactions.CountAsync(x => x.ProductId == nullStockProductId)).ShouldBe(1);
            }
        }
        finally
        {
            await DropDatabaseAsync(connectionString);
        }
    }

    private static ApplicationDbContext CreateContext(string connectionString) =>
        new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(connectionString)
            .Options);

    private static string CreateIsolatedDatabaseConnectionString(string serverConnectionString)
    {
        var builder = new SqlConnectionStringBuilder(serverConnectionString)
        {
            InitialCatalog = $"InventoryMigration_{Guid.NewGuid():N}"
        };

        return builder.ConnectionString;
    }

    private static async Task CreateDatabaseAsync(string connectionString)
    {
        var builder = new SqlConnectionStringBuilder(connectionString);
        var databaseName = builder.InitialCatalog;
        builder.InitialCatalog = "master";

        await using var connection = new SqlConnection(builder.ConnectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = $"CREATE DATABASE [{databaseName}]";
        await command.ExecuteNonQueryAsync();
    }

    private static async Task DropDatabaseAsync(string connectionString)
    {
        var builder = new SqlConnectionStringBuilder(connectionString);
        var databaseName = builder.InitialCatalog;
        builder.InitialCatalog = "master";

        SqlConnection.ClearAllPools();

        await using var connection = new SqlConnection(builder.ConnectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = $"ALTER DATABASE [{databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{databaseName}]";
        await command.ExecuteNonQueryAsync();
    }
}
