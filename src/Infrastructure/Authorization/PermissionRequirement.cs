using CleanArchitecture.Northwind.Application.Common.Models;
using Microsoft.AspNetCore.Authorization;

namespace CleanArchitecture.Northwind.Infrastructure.Authorization;

public sealed class PermissionRequirement : IAuthorizationRequirement
{
    public Permission Required { get; }
    public PermissionRequirement(Permission required) => Required = required;
}
