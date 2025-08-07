using System.Security.Cryptography;
using CleanArchitecture.Northwind.Domain.Constants;
using CleanArchitecture.Northwind.Domain.Entities;
using CleanArchitecture.Northwind.Domain.Entities.Identity;
using CleanArchitecture.Northwind.Domain.Enums;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Northwind.Infrastructure.Data;
public static class InitialiserExtensions
{
    public static async Task InitialiseDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var initialiser = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitialiser>();

        await initialiser.InitialiseAsync();

        await initialiser.SeedAsync();
    }
}

public class ApplicationDbContextInitialiser
{
    private readonly ILogger<ApplicationDbContextInitialiser> _logger;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;

    public ApplicationDbContextInitialiser(ILogger<ApplicationDbContextInitialiser> logger, ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task InitialiseAsync()
    {
        try
        {
            await _context.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initialising the database.");
            throw;
        }
    }

    public async Task SeedAsync()
    {
        try
        {
            await TrySeedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    public async Task TrySeedAsync()
    {
        // Default roles
        await AddDefaultRolesAsync();

        // Default users
        var adminUserId = await AddAdministratorUserAsync();
        await AddDefaultUserAsync(Roles.Sales, adminUserId);
        await AddDefaultUserAsync(Roles.Warehouse, adminUserId);
        await AddDefaultUserAsync(Roles.Purchase, adminUserId);
        await AddDefaultUserAsync(Roles.Finance, adminUserId);
        await AddDefaultUserAsync(Roles.CustomerService, adminUserId);

        // Default data
        // Seed, if necessary
        if (!_context.TodoLists.Any())
        {
            _context.TodoLists.Add(new TodoList
            {
                Title = "Todo List",
                Items =
                {
                    new TodoItem { Title = "Make a todo list 📃" },
                    new TodoItem { Title = "Check off the first item ✅" },
                    new TodoItem { Title = "Realise you've already done two things on the list! 🤯"},
                    new TodoItem { Title = "Reward yourself with a nice, long nap 🏆" },
                }
            });

            await _context.SaveChangesAsync();
        }
    }

    public async Task AddDefaultRolesAsync()
    {
        var administratorRole = new ApplicationRole(Roles.Administrator, 1, "系統管理員");

        if (_roleManager.Roles.All(r => r.Name != administratorRole.Name))
        {
            await _roleManager.CreateAsync(administratorRole);
        }


        var role = new ApplicationRole(Roles.Sales, 2, "銷售代表");

        if (_roleManager.Roles.All(r => r.Name != role.Name))
        {
            await _roleManager.CreateAsync(role);
        }

        role = new ApplicationRole(Roles.Warehouse, 3, "倉庫經理");

        if (_roleManager.Roles.All(r => r.Name != role.Name))
        {
            await _roleManager.CreateAsync(role);
        }

        role = new ApplicationRole(Roles.Purchase, 4, "採購代理");

        if (_roleManager.Roles.All(r => r.Name != role.Name))
        {
            await _roleManager.CreateAsync(role);
        }

        role = new ApplicationRole(Roles.Finance, 5, "財務人員");

        if (_roleManager.Roles.All(r => r.Name != role.Name))
        {
            await _roleManager.CreateAsync(role);
        }

        role = new ApplicationRole(Roles.CustomerService, 6, "客戶服務代表");

        if (_roleManager.Roles.All(r => r.Name != role.Name))
        {
            await _roleManager.CreateAsync(role);
        }
    }

    public async Task<string> AddAdministratorUserAsync()
    {
        var administrator = new ApplicationUser { UserName = "administrator@localhost", Email = "administrator@localhost" };
        ApplicationUser? user;

        if (_userManager.Users.All(u => u.UserName != administrator.UserName))
        {
            await _userManager.CreateAsync(administrator, "Administrator1!");
            if (!string.IsNullOrWhiteSpace(Roles.Administrator))
            {
                await _userManager.AddToRolesAsync(administrator, new[] { Roles.Administrator });
            }

            user = await _userManager.FindByEmailAsync(administrator.Email);
            user.EmailConfirmed = true;
            _context.UserProfiles.Add(new ApplicationUserProfile
            {
                UserId = user.Id,
                FullName = "ADMINISTRATOR",
                Gender = Gender.Male,
                Title = "管理員",
                Status = Status.Enabled,
                Created = DateTime.Now,
                CreatedBy = user.Id,
            });

            await _context.SaveChangesAsync();
        }
        else
        {
            user = await _userManager.FindByEmailAsync(administrator.Email);
        }

        return user.Id;
    }

    public async Task AddDefaultUserAsync(string roleName, string adminUserId)
    {
        var defaultUser = new ApplicationUser { UserName = $"{roleName.ToLower()}@localhost", Email = $"{roleName.ToLower()}@localhost" };

        if (_userManager.Users.All(u => u.UserName != defaultUser.UserName))
        {
            await _userManager.CreateAsync(defaultUser, "P@ssw0rdTest");
            await _userManager.AddToRolesAsync(defaultUser, new[] { roleName });

            var user = await _userManager.FindByEmailAsync(defaultUser.Email);
            user.EmailConfirmed = true;
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                byte[] randomNumber = new byte[1];
                rng.GetBytes(randomNumber);

                _context.UserProfiles.Add(new ApplicationUserProfile
                {
                    UserId = user.Id,
                    FullName = roleName.ToUpper(),
                    Gender = (Gender)(randomNumber[0] % 3),
                    Title = roleName,
                    IsTotpEnabled = false,
                    Status = Status.Enabled,
                    Created = DateTime.Now,
                    CreatedBy = adminUserId,
                });
            }

            await _context.SaveChangesAsync();
        }
    }
}
