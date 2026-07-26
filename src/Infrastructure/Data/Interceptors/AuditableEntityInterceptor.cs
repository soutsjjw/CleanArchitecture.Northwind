using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace CleanArchitecture.Northwind.Infrastructure.Data.Interceptors;

public class AuditableEntityInterceptor : SaveChangesInterceptor
{
    private readonly IUser _user;
    private readonly TimeProvider _dateTime;

    public AuditableEntityInterceptor(
        IUser user,
        TimeProvider dateTime)
    {
        _user = user;
        _dateTime = dateTime;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        UpdateEntities(eventData.Context);

        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        UpdateEntities(eventData.Context);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public void UpdateEntities(DbContext? context)
    {
        if (context == null) return;

        foreach (var entry in context.ChangeTracker.Entries()
                     .Where(entry => entry.Entity is BaseAuditableEntity || IsGenericAuditableEntity(entry.Entity.GetType())))
        {
            if (entry.State is EntityState.Added or EntityState.Modified || entry.HasChangedOwnedEntities())
            {
                var utcNow = _dateTime.GetUtcNow();
                var currentUserId = _user.Id;
                if (entry.Entity is IAuditableEntity auditableEntity)
                {
                    if (entry.State == EntityState.Added)
                    {
                        if (!string.IsNullOrWhiteSpace(currentUserId))
                        {
                            auditableEntity.CreatedBy = currentUserId;
                        }
                        auditableEntity.Created = utcNow.UtcDateTime;
                    }
                    if (!string.IsNullOrWhiteSpace(currentUserId))
                    {
                        auditableEntity.LastModifiedBy = currentUserId;
                    }
                    auditableEntity.LastModified = utcNow.UtcDateTime;
                    continue;
                }

                if (entry.State == EntityState.Added)
                {
                    if (!string.IsNullOrWhiteSpace(currentUserId))
                    {
                        entry.CurrentValues[nameof(BaseAuditableEntity.CreatedBy)] = currentUserId;
                    }
                    entry.CurrentValues[nameof(BaseAuditableEntity.Created)] = utcNow;
                }
                if (!string.IsNullOrWhiteSpace(currentUserId))
                {
                    entry.CurrentValues[nameof(BaseAuditableEntity.LastModifiedBy)] = currentUserId;
                }
                entry.CurrentValues[nameof(BaseAuditableEntity.LastModified)] = utcNow;
            }
        }
    }

    private static bool IsGenericAuditableEntity(Type entityType) =>
        entityType.BaseType is { IsGenericType: true } baseType &&
        baseType.GetGenericTypeDefinition() == typeof(BaseAuditableEntity<>);
}

public static class Extensions
{
    public static bool HasChangedOwnedEntities(this EntityEntry entry) =>
        entry.References.Any(r =>
            r.TargetEntry != null &&
            r.TargetEntry.Metadata.IsOwned() &&
            (r.TargetEntry.State == EntityState.Added || r.TargetEntry.State == EntityState.Modified));
}
