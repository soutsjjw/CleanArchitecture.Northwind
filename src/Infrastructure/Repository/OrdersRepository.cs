using System.Data;
using CleanArchitecture.Northwind.Application.Common.Interfaces.Repository;
using CleanArchitecture.Northwind.Domain.Entities;
using Dapper;

namespace CleanArchitecture.Northwind.Infrastructure.Repository;

public class OrdersRepository : IRepository<Order>
{
    private readonly IDbConnection _dbConnection;

    public OrdersRepository(IDbConnection dbConnection)
    {
        _dbConnection = dbConnection;
    }

    public async Task AddAsync(Order entity)
    {
        var sql = @"
            INSERT INTO Orders
            (CustomerID, EmployeeID, OrderDate, RequiredDate, ShippedDate, ShipVia, Freight, ShipName, ShipAddress, ShipCity, ShipRegion, ShipPostalCode, ShipCountry, Created, CreatedBy)
            VALUES
            (@CustomerId, @EmployeeId, @OrderDate, @RequiredDate, @ShippedDate, @ShipVia, @Freight, @ShipName, @ShipAddress, @ShipCity, @ShipRegion, @ShipPostalCode, @ShipCountry, @Created, @CreatedBy);
            SELECT CAST(SCOPE_IDENTITY() as int);";

        var id = await _dbConnection.ExecuteScalarAsync<int>(sql, entity);
        entity.Id = id;
    }

    public async Task DeleteAsync(object id)
    {
        var sql = "DELETE FROM Orders WHERE OrderID = @Id";
        await _dbConnection.ExecuteAsync(sql, new { Id = id });
    }

    public async Task<IEnumerable<Order>> GetAllAsync()
    {
        var sql = @"
SELECT OrderID AS Id,
            CustomerID,
            DepartmentId,
            OfficeId,
            EmployeeID,
            OrderDate,
            RequiredDate,
            ShippedDate,
            ShipVia,
            Freight,
            ShipName,
            ShipAddress,
            ShipCity,
            ShipRegion,
            ShipPostalCode,
            ShipCountry,
            Created,
            CreatedBy,
            LastModified,
            LastModifiedBy
FROM Orders";
        return await _dbConnection.QueryAsync<Order>(sql);
    }

    public async Task<Order?> GetByIdAsync(object id)
    {
        var sql = "SELECT * FROM Orders WHERE OrderID = @Id";
        return await _dbConnection.QueryFirstOrDefaultAsync<Order>(sql, new { Id = id });
    }

    public async Task UpdateAsync(Order entity)
    {
        var sql = @"
            UPDATE Orders SET
                CustomerID = @CustomerId,
                EmployeeID = @EmployeeId,
                OrderDate = @OrderDate,
                RequiredDate = @RequiredDate,
                ShippedDate = @ShippedDate,
                ShipVia = @ShipVia,
                Freight = @Freight,
                ShipName = @ShipName,
                ShipAddress = @ShipAddress,
                ShipCity = @ShipCity,
                ShipRegion = @ShipRegion,
                ShipPostalCode = @ShipPostalCode,
                ShipCountry = @ShipCountry,
                LastModified = @LastModified,
                LastModifiedBy = @LastModifiedBy
            WHERE OrderID = @Id";
        await _dbConnection.ExecuteAsync(sql, entity);
    }
}
