using CleanArchitecture.Northwind.Domain.Entities;

namespace CleanArchitecture.Northwind.Application.Common.Interfaces.Database;

public interface IOrdersService
{
    Task<List<Order>> GetAllAsync();

    Task<Order> GetByIdAsync(int orderId);
}
