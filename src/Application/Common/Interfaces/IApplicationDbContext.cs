using CleanArchitecture.Northwind.Domain.Entities;
using CleanArchitecture.Northwind.Domain.Entities.Identity;

namespace CleanArchitecture.Northwind.Application.Common.Interfaces;
public interface IApplicationDbContext
{
    #region Identity

    DbSet<ApplicationUserProfile> UserProfiles { get; }

    #endregion

    DbSet<TodoList> TodoLists { get; }

    DbSet<TodoItem> TodoItems { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
