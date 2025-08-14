using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Common.Models;
using CleanArchitecture.Northwind.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Northwind.Application.Features.Role.Commands.CreateRole;

public class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly ILogger<CreateRoleCommandHandler> _logger;

    public CreateRoleCommandHandler(IApplicationDbContext context,
        RoleManager<ApplicationRole> roleManager,
        ILogger<CreateRoleCommandHandler> logger)
    {
        _context = context;
        _roleManager = roleManager;
        _logger = logger;
    }

    public async Task<Result> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        if (await _roleManager.RoleExistsAsync(request.Name))
        {
            return await Result.FailureAsync($"角色名稱 '{request.Name}' 已存在。");
        }

        var sort = _roleManager.Roles.Max(x => x.Sort) + 1;

        var result = await _roleManager.CreateAsync(new ApplicationRole(request.Name, sort, request.Description));

        if (result.Succeeded)
        {
            return Result.Success();
        }
        else
        {
            _logger.LogError(String.Join("，", result.Errors.Select(e => e.Description).ToArray()));

            return await Result.FailureAsync($"建立 '{request.Name}' 角色失敗");
        }
    }
}
