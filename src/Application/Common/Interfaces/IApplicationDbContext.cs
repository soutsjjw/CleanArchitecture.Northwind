using CleanArchitecture.Northwind.Domain.Entities;
using CleanArchitecture.Northwind.Domain.Entities.Identity;

namespace CleanArchitecture.Northwind.Application.Common.Interfaces;
public interface IApplicationDbContext
{
    #region Identity

    DbSet<ApplicationUser> Users { get; }

    DbSet<ApplicationUserProfile> UserProfiles { get; }

    DbSet<ApplicationUserPasswordHistory> UserPasswordHistories { get; }

    DbSet<Department> Departments { get; }

    DbSet<Office> Offices { get; }

    #endregion

    #region Northwind

    DbSet<Category> Categories { get; }

    DbSet<Customer> Customers { get; }

    DbSet<CustomerCustomerDemo> CustomerCustomerDemos { get; }

    DbSet<CustomerDemographic> CustomerDemographics { get; }

    DbSet<Employee> Employees { get; }

    DbSet<EmployeeTerritory> EmployeeTerritories { get; }

    DbSet<Order> Orders { get; }

    DbSet<OrderDetail> OrderDetails { get; }

    DbSet<Product> Products { get; }

    DbSet<Region> Regions { get; }

    DbSet<Shipper> Shippers { get; }

    DbSet<Supplier> Suppliers { get; }

    DbSet<Territory> Territories { get; }

    #endregion

    DbSet<TodoList> TodoLists { get; }

    DbSet<TodoItem> TodoItems { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
