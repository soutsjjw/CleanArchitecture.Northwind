using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Common.Interfaces.Database;
using CleanArchitecture.Northwind.Application.Common.Interfaces.Repository;
using CleanArchitecture.Northwind.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Northwind.Infrastructure.Services.Database;

public class OrdersService : IOrdersService
{
    private readonly IApplicationDbContext _context;
    private IRepository<Order> _orderRepository;

    public OrdersService(IApplicationDbContext context,
        IRepository<Order> orderRepository)
    {
        _context = context;
        _orderRepository = orderRepository;
    }

    public async Task<List<Order>> GetAllAsync()
    {
        //return await _context.Orders.Take(100).ToListAsync();

        var list = (await _orderRepository.GetAllAsync()).ToList();

        return list;
    }

    public async Task<Order> GetByIdAsync(int orderId)
    {
        return await _context.Orders.SingleOrDefaultAsync(x => x.Id == orderId);
    }
}
