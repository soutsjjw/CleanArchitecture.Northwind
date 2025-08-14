using CleanArchitecture.Northwind.Application.Common.DTOs;
using CleanArchitecture.Northwind.Domain.Entities.Identity;

namespace CleanArchitecture.Northwind.Application.Common.Interfaces.Repository;

public interface IUserProfileRepository : IRepository<ApplicationUserProfile>
{
    Task<IEnumerable<ApplicationUserProfile>> GetUserProfilesAsync(AccountConditionDto condition);

    // 明確實作 IRepository<T>.AddAsync → 不會出現在類別的公開 API
    Task IRepository<ApplicationUserProfile>.AddAsync(ApplicationUserProfile entity)
        => throw new NotSupportedException("UserProfile 不支援新增。");

    // 明確實作 IRepository<T>.GetAllAsync → 不會出現在類別的公開 API
    Task<IEnumerable<ApplicationUserProfile>> IRepository<ApplicationUserProfile>.GetAllAsync()
        => throw new NotSupportedException("UserProfile 不支援查詢。");

    // 明確實作 IRepository<T>.GetByIdAsync → 不會出現在類別的公開 API
    Task<ApplicationUserProfile?> IRepository<ApplicationUserProfile>.GetByIdAsync(object id)
        => throw new NotSupportedException("UserProfile 不支援查詢。");

    // 明確實作 IRepository<T>.UpdateAsync → 不會出現在類別的公開 API
    Task IRepository<ApplicationUserProfile>.UpdateAsync(ApplicationUserProfile entity)
        => throw new NotSupportedException("UserProfile 不支援更新。");

    // 明確實作 IRepository<T>.DeleteAsync → 不會出現在類別的公開 API
    Task IRepository<ApplicationUserProfile>.DeleteAsync(object id)
        => throw new NotSupportedException("UserProfile 不支援刪除。");
}
