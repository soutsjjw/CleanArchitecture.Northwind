using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;

namespace CleanArchitecture.Northwind.Application.FunctionalTests;

[SetUpFixture]
public class FunctionalTestSetup
{
    private const string TestConnectionStringEnvironmentVariable = "CAN_TestConnectionString";

    internal static IServiceScopeFactory ScopeFactory { get; private set; } = null!;
    internal static DatabaseResetter? DbResetter { get; private set; }

    private static WebApiFactory? _factory;
    private static MsSqlContainer? _database;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _database = new MsSqlBuilder().Build();

        await _database.StartAsync();

        var connectionString = CreateApplicationDatabaseConnectionString(_database.GetConnectionString());

        Environment.SetEnvironmentVariable(TestConnectionStringEnvironmentVariable, connectionString);

        _factory = new WebApiFactory(TestConnectionStringEnvironmentVariable);
        ScopeFactory = _factory.Services.GetRequiredService<IServiceScopeFactory>();
        DbResetter = await DatabaseResetter.CreateAsync(connectionString);
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        if (DbResetter is not null) await DbResetter.DisposeAsync();
        if (_factory is not null) await _factory.DisposeAsync();
        if (_database is not null) await _database.DisposeAsync();

        Environment.SetEnvironmentVariable(TestConnectionStringEnvironmentVariable, null);
    }

    private static string CreateApplicationDatabaseConnectionString(string connectionString)
    {
        var builder = new SqlConnectionStringBuilder(connectionString)
        {
            InitialCatalog = Services.Database
        };

        return builder.ConnectionString;
    }
}
