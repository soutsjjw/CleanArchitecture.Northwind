using CleanArchitecture.Northwind.Domain.Enums;

namespace CleanArchitecture.Northwind.Application.Common.Models;

public readonly record struct Permission(string Module, string Operation, DataScope? Scope);
