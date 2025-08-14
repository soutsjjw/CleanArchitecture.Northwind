﻿namespace CleanArchitecture.Northwind.Application.Common.Interfaces.Repository;

public interface IRepository<T>
{
    Task<T?> GetByIdAsync(object id);
    Task<IEnumerable<T>> GetAllAsync();
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(object id);
}
