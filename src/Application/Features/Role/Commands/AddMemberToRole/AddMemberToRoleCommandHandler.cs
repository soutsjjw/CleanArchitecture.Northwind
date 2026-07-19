using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Common.Models;
using CleanArchitecture.Northwind.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;

namespace CleanArchitecture.Northwind.Application.Features.Role.Commands.AddMemberToRole;

public class AddMemberToRoleCommandHandler : IRequestHandler<AddMemberToRoleCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;

    public AddMemberToRoleCommandHandler(IApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<Result> Handle(AddMemberToRoleCommand request, CancellationToken cancellationToken)
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

        if (await _userManager.IsInRoleAsync(user, role.Name ?? ""))
        {
            return await Result.FailureAsync("使用者已加入角色");
        }

        var result = await _userManager.AddToRoleAsync(user, role.Name);

        if (result.Succeeded)
        {
            return await Result.SuccessAsync("使用者已成功加入角色");
        }

        return await Result.FailureAsync(
            result.Errors.Select(e => e.Description),
            500
        );
    }
}
