using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Common.Models;
using CleanArchitecture.Northwind.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Northwind.Application.Features.Role.Queries.GetAllRoles;

public class GetAllRolesQueryHandler : IRequestHandler<GetAllRolesQuery, Result<List<RolesDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly ILogger<GetAllRolesQueryHandler> _logger;

    public GetAllRolesQueryHandler(IApplicationDbContext context,
        RoleManager<ApplicationRole> roleManager,
        ILogger<GetAllRolesQueryHandler> logger)
    {
        _context = context;
        _roleManager = roleManager;
        _logger = logger;
    }

    public async Task<Result<List<RolesDto>>> Handle(GetAllRolesQuery request, CancellationToken cancellationToken)
    {
        var roleList = _roleManager.Roles.Select(x => new RolesDto
        {
            Id = x.Id,
            Name = x.Name ?? "",
            Description = x.Description ?? "",
            Sort = x.Sort
        });

        return await Result<List<RolesDto>>.SuccessAsync(roleList.ToList());
    }
}
