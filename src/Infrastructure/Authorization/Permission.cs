namespace CleanArchitecture.Northwind.Infrastructure.Authorization;

public readonly record struct PermissionPattern(string Module, string Action, string Scope);
