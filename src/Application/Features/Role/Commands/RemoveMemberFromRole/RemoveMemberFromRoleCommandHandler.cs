using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Common.Models;
using CleanArchitecture.Northwind.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Northwind.Application.Features.Role.Commands.RemoveMemberFromRole;

public class RemoveMemberFromRoleCommandHandler : IRequestHandler<RemoveMemberFromRoleCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly ILogger<RemoveMemberFromRoleCommandHandler> _logger;

    public RemoveMemberFromRoleCommandHandler(IApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        ILogger<RemoveMemberFromRoleCommandHandler> logger)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
    }

    public async Task<Result> Handle(RemoveMemberFromRoleCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId.ToString());
        if (user == null)
        {
            return await Result.FailureAsync("未找到使用者資料");
        }

        var role = await _roleManager.FindByIdAsync(request.RoleId.ToString());
        if (role == null)
        {
            return await Result.FailureAsync("未找到角色資料");
        }

        var result = await _userManager.RemoveFromRoleAsync(user, role.Name);

        if (result.Succeeded)
        {
            return Result.Success();
        }
        else
        {
            _logger.LogError(String.Join("，", result.Errors.Select(e => e.Description).ToArray()));

            return await Result.FailureAsync($"從{role.Name}角色群組中移除使用者失敗");
        }
    }
}
