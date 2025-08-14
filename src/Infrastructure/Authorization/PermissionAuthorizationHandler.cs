using CleanArchitecture.Northwind.Application.Common.Interfaces.Identity;
using CleanArchitecture.Northwind.Domain.Constants;
using CleanArchitecture.Northwind.Domain.Enums;
using Microsoft.AspNetCore.Authorization;

namespace CleanArchitecture.Northwind.Infrastructure.Authorization;

public sealed class PermissionAuthorizationHandler
    : AuthorizationHandler<PermissionRequirement>
{
    private readonly IPermissionResolver _resolver;

    public PermissionAuthorizationHandler(IPermissionResolver resolver)
        => _resolver = resolver;

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context, PermissionRequirement req)
    {
        if (context.User.IsInRole(Roles.SystemAdmin) || context.User.IsInRole(Roles.Administrator))
        {
            context.Succeed(req);
            return;
        }

        var granted = await _resolver.GetForUserAsync(context.User);

        if (string.IsNullOrEmpty(req.Required.Operation) && req.Required.Scope == DataScope.None)
        {
            if (granted.Any(x => x.Module == req.Required.Module))
            {
                context.Succeed(req);
                return;
            }
        }

        if (granted.Any(x => x.Module == req.Required.Module && x.Operation == nameof(DataScope.System)))
        {
            context.Succeed(req);
            return;
        }

        foreach (var g in granted)
        {
            var moduleOk = g.Module.Equals(req.Required.Module, StringComparison.OrdinalIgnoreCase) || g.Module == "*";
            var operationOk = g.Operation.Equals(req.Required.Operation, StringComparison.OrdinalIgnoreCase) || g.Operation == "*";
            var scopeOk = Rank(g.Scope) >= Rank(req.Required.Scope);

            if (moduleOk && operationOk && scopeOk)
            {
                context.Succeed(req);
                return;
            }
        }
    }

    private static int Rank(DataScope? s) => s switch
    {
        DataScope.Self => 0,
        DataScope.Office => 1,
        DataScope.Department => 2,
        DataScope.All => 3,
        DataScope.System => 4,
        _ => -1
    };
}
