using System.Globalization;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
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
        #region Identity Seed

        // Default roles
        await AddDefaultRolesAsync();

        // Default users
        await AddSystemAdminUserAsync();
        var adminUserId = await AddAdministratorUserAsync();

        await AddDefaultUserAsync("SalesNAManager", adminUserId, "北區業務管理", 1, 1);
        await AddDefaultUserAsync("SalesNAA", adminUserId, "北區業務A", 1, 1);
        await AddDefaultUserAsync("SalesNAB", adminUserId, "北區業務B", 1, 1);
        await AddDefaultUserAsync("SalesNAC", adminUserId, "北區業務C", 1, 1);

        await AddDefaultUserAsync("SalesCAManager", adminUserId, "中區業務管理", 1, 2);
        await AddDefaultUserAsync("SalesCAA", adminUserId, "中區業務A", 1, 2);
        await AddDefaultUserAsync("SalesCAB", adminUserId, "中區業務B", 1, 2);
        await AddDefaultUserAsync("SalesCAC", adminUserId, "中區業務C", 1, 2);

        await AddDefaultUserAsync("SalesSAManager", adminUserId, "南區業務管理", 1, 3);
        await AddDefaultUserAsync("SalesSAA", adminUserId, "南區業務A", 1, 3);
        await AddDefaultUserAsync("SalesSAB", adminUserId, "南區業務B", 1, 3);
        await AddDefaultUserAsync("SalesSAC", adminUserId, "南區業務C", 1, 3);

        await AddDefaultUserAsync("SalesEAManager", adminUserId, "東區業務管理", 1, 4);
        await AddDefaultUserAsync("SalesEAA", adminUserId, "東區業務A", 1, 4);
        await AddDefaultUserAsync("SalesEAB", adminUserId, "東區業務B", 1, 4);
        await AddDefaultUserAsync("SalesEAC", adminUserId, "東區業務C", 1, 4);

        await AddDefaultUserAsync("HrTaManager", adminUserId, "招募與訓練組長", 6, 1);
        await AddDefaultUserAsync("HrTaA", adminUserId, "招募與訓練組員", 6, 1);
        await AddDefaultUserAsync("HrTaB", adminUserId, "招募與訓練組員", 6, 1);
        await AddDefaultUserAsync("HrTaC", adminUserId, "招募與訓練組員", 6, 1);

        await AddDefaultUserAsync("HrCbManager", adminUserId, "薪資與福利組組長", 6, 2);
        await AddDefaultUserAsync("HrCbA", adminUserId, "薪資與福利組員", 6, 2);
        await AddDefaultUserAsync("HrCbB", adminUserId, "薪資與福利組員", 6, 2);
        await AddDefaultUserAsync("HrCbC", adminUserId, "薪資與福利組員", 6, 2);

        await AddDepartmentAsync();

        await AddOfficeAsync();

        #endregion

        #region Northwind Seed

        var instnwndPath = FindInstNwnd();
        var sqlText = await File.ReadAllTextAsync(instnwndPath);

        await AddCategoriesAsync(adminUserId, sqlText);

        await AddCustomersAsync(adminUserId, sqlText);

        await AddEmployeesAsync(adminUserId, sqlText);

        await AddSuppliersAsync(adminUserId, sqlText);

        await AddProductsAsync(adminUserId, sqlText);

        await AddShippersAsync(adminUserId, sqlText);

        await AddOrdersAsync(adminUserId, sqlText);

        await AddOrderDetailsAsync(adminUserId, sqlText);

        await AddRegionsAsync(adminUserId, sqlText);

        await AddTerritoriesAsync(adminUserId, sqlText);

        await AddEmployeeTerritoriesAsync(adminUserId, sqlText);

        #endregion

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

    #region Identity Seed

    public async Task AddDefaultRolesAsync()
    {
        var role = new ApplicationRole(Roles.SystemAdmin, 1, "系統管理員");

        if (_roleManager.Roles.All(r => r.Name != role.Name))
        {
            await _roleManager.CreateAsync(role);
        }

        role = new ApplicationRole(Roles.Administrator, 2, "管理員");

        if (_roleManager.Roles.All(r => r.Name != role.Name))
        {
            await _roleManager.CreateAsync(role);
        }
    }

    public async Task<string> AddSystemAdminUserAsync()
    {
        var systemAdmin = new ApplicationUser { UserName = "systemadmin@localhost", Email = "systemadmin@localhost" };
        ApplicationUser? user;

        if (_userManager.Users.All(u => u.UserName != systemAdmin.UserName))
        {
            await _userManager.CreateAsync(systemAdmin, "SystemAdmin1!");
            if (!string.IsNullOrWhiteSpace(Roles.SystemAdmin))
            {
                await _userManager.AddToRolesAsync(systemAdmin, new[] { Roles.SystemAdmin });
            }

            user = await _userManager.FindByEmailAsync(systemAdmin.Email);
            user.EmailConfirmed = true;
            _context.UserProfiles.Add(new ApplicationUserProfile
            {
                UserId = user.Id,
                FullName = "SYSTEMADMIN",
                Gender = Gender.Male,
                Title = "系統管理員",
                Status = Status.Enabled,
                Created = DateTime.Now,
                CreatedBy = user.Id,
            });

            await _context.SaveChangesAsync();
        }
        else
        {
            user = await _userManager.FindByEmailAsync(systemAdmin.Email);
        }

        return user.Id;
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

    public async Task AddDefaultUserAsync(string userName, string adminUserId, string title, int departmentId, int officeId)
    {
        var defaultUser = new ApplicationUser { UserName = $"{userName.ToLower()}@localhost", Email = $"{userName.ToLower()}@localhost" };

        if (_userManager.Users.All(u => u.UserName != defaultUser.UserName))
        {
            await _userManager.CreateAsync(defaultUser, "P@ssw0rdTest");

            var user = await _userManager.FindByEmailAsync(defaultUser.Email);
            user.EmailConfirmed = true;
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                byte[] randomNumber = new byte[1];
                rng.GetBytes(randomNumber);

                _context.UserProfiles.Add(new ApplicationUserProfile
                {
                    UserId = user.Id,
                    FullName = userName.ToUpper(),
                    Gender = (Gender)(randomNumber[0] % 3),
                    Title = title,
                    DepartmentId = departmentId,
                    OfficeId = officeId,
                    IsTotpEnabled = false,
                    Status = Status.Enabled,
                    Created = DateTime.Now,
                    CreatedBy = adminUserId,
                });
            }

            await _context.SaveChangesAsync();
        }
    }

    public async Task AddDepartmentAsync()
    {
        if (_context.Departments.Any())
            return;

        _context.Departments.AddRange(new List<Department>
        {
            new Department { DepartmentId = 1, DeptCode = "SLS", DeptName = "業務處" },
            new Department { DepartmentId = 2, DeptCode = "PRC", DeptName = "採購處" },
            new Department { DepartmentId = 3, DeptCode = "LOG", DeptName = "倉儲物流處" },
            new Department { DepartmentId = 4, DeptCode = "FIN", DeptName = "財務處" },
            new Department { DepartmentId = 5, DeptCode = "IT",  DeptName = "資訊處" },
            new Department { DepartmentId = 6, DeptCode = "HR",  DeptName = "人力資源處" }
        });

        using var tx = await _context.Database.BeginTransactionAsync();
        await _context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT [dbo].[Departments] ON");

        await _context.SaveChangesAsync();

        await _context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT [dbo].[Departments] OFF");
        await tx.CommitAsync();
    }

    public async Task AddOfficeAsync()
    {
        if (_context.Offices.Any())
            return;

        _context.Offices.AddRange(new List<Office>
        {
            // SLS
            new Office { Id = 1, OfficeId = 1, DepartmentId = 1, OfficeCode = "SLS-NA", OfficeName = "北區" },
            new Office { Id = 2, OfficeId = 2, DepartmentId = 1, OfficeCode = "SLS-CA", OfficeName = "中區" },
            new Office { Id = 3, OfficeId = 3, DepartmentId = 1, OfficeCode = "SLS-SA", OfficeName = "南區" },
            new Office { Id = 4, OfficeId = 4, DepartmentId = 1, OfficeCode = "SLS-EA", OfficeName = "東區" },

            // PRC
            new Office { Id = 5, OfficeId = 1, DepartmentId = 2, OfficeCode = "PRC-RM",  OfficeName = "原物料採購組" },
            new Office { Id = 6, OfficeId = 2, DepartmentId = 2, OfficeCode = "PRC-IND", OfficeName = "間接/一般採購組" },
            new Office { Id = 7, OfficeId = 3, DepartmentId = 2, OfficeCode = "PRC-SUP", OfficeName = "供應商管理組" },

            // LOG
            new Office { Id = 8, OfficeId = 1, DepartmentId = 3, OfficeCode = "LOG-OUT", OfficeName = "出貨與配送組" },
            new Office { Id = 9, OfficeId = 2, DepartmentId = 3, OfficeCode = "LOG-INV", OfficeName = "庫存管理組" },
            new Office { Id = 10, OfficeId = 3, DepartmentId = 3, OfficeCode = "LOG-IMP", OfficeName = "進口與關務組" },

            // FIN
            new Office { Id = 11, OfficeId = 1, DepartmentId = 4, OfficeCode = "FIN-AR",  OfficeName = "應收帳款組" },
            new Office { Id = 12, OfficeId = 2, DepartmentId = 4, OfficeCode = "FIN-AP",  OfficeName = "應付帳款組" },
            new Office { Id = 13, OfficeId = 3, DepartmentId = 4, OfficeCode = "FIN-CST", OfficeName = "成本與預算控管組" },

            // IT
            new Office { Id = 14, OfficeId = 1, DepartmentId = 5, OfficeCode = "IT-APP", OfficeName = "應用系統組" },
            new Office { Id = 15, OfficeId = 2, DepartmentId = 5, OfficeCode = "IT-DBI", OfficeName = "資料庫與商業智慧組" },
            new Office { Id = 16, OfficeId = 3, DepartmentId = 5, OfficeCode = "IT-INF", OfficeName = "基礎架構組" },

            // HR
            new Office { Id = 17, OfficeId = 1, DepartmentId = 6, OfficeCode = "HR-TA",  OfficeName = "招募與訓練組" },
            new Office { Id = 18, OfficeId = 2, DepartmentId = 6, OfficeCode = "HR-C&B", OfficeName = "薪資與福利組" }
        });

        using var tx = await _context.Database.BeginTransactionAsync();
        await _context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT [dbo].[Offices] ON");

        await _context.SaveChangesAsync();

        await _context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT [dbo].[Offices] OFF");
        await tx.CommitAsync();
    }

    #endregion

    #region Northwind Seed

    public async Task AddCategoriesAsync(string adminUserId, string sqlText)
    {
        if (_context.Categories.Any())
            return;

        // 允許：INSERT 或 INSERT INTO；"Categories" 或 [Categories]；欄位為 "CategoryID","CategoryName","Description","Picture"
        // 重點：(?<pic>0x[0-9A-Fa-f]*) 允許 0x 後面是 0~多個 hex 字元（你的檔是純 0x）
        var pattern = new Regex(
            @"INSERT\s+(?:INTO\s+)?(?:""Categories""|\[Categories\]|(?:\[dbo\]\.)?\[Categories\])\s*"
          + @"\(\s*(?:""CategoryID""|\[CategoryID\])\s*,\s*(?:""CategoryName""|\[CategoryName\])\s*,\s*(?:""Description""|\[Description\])\s*,\s*(?:""Picture""|\[Picture\])\s*\)\s*"
          + @"VALUES\s*\(\s*(?<id>\d+)\s*,\s*'(?<name>(?:''|[^'])*)'\s*,\s*'(?<desc>(?:''|[^'])*)'\s*,\s*(?<pic>0x[0-9A-Fa-f]*)\s*\)",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Multiline);

        static byte[] HexToBytes(string hex)
        {
            if (hex.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                hex = hex[2..];
            if (hex.Length == 0)
                return Array.Empty<byte>(); // 處理 0x（零長度）
                                            // 可選：若長度為奇數可在此做保護或拋錯
            var bytes = new byte[hex.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
                bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            return bytes;
        }

        string Unescape(string s) => s.Replace("''", "'");

        var matches = pattern.Matches(sqlText);
        if (matches.Count == 0)
            throw new InvalidOperationException("在 instnwnd.sql 中找不到 Categories 的 INSERT（含 Picture）。");

        var now = DateTimeOffset.UtcNow;

        using var tx = await _context.Database.BeginTransactionAsync();
        await _context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT [dbo].[Categories] ON");

        var categories = matches
            .Select(m => new Category
            {
                Id = int.Parse(m.Groups["id"].Value),   // 依腳本中的 CategoryID
                CategoryName = Unescape(m.Groups["name"].Value),
                Description = Unescape(m.Groups["desc"].Value),
                Picture = HexToBytes(m.Groups["pic"].Value), // 0x -> 空位元組
            })
            .OrderBy(c => c.Id) // 保險起見，按 ID 排一下
            .ToList();

        _context.Categories.AddRange(categories);
        await _context.SaveChangesAsync();

        await _context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT [dbo].[Categories] OFF");
        await tx.CommitAsync();
    }

    public async Task AddCustomersAsync(string adminUserId, string sqlText)
    {
        if (_context.Customers.Any())
            return;

        var now = DateTimeOffset.UtcNow;

        // INSERT "Customers" VALUES('ALFKI','Alfreds ...','Maria ...','Sales ...','Obere Str. 57','Berlin',NULL,'12209','Germany','030-0074321','030-0076545')
        // 支援：
        // - INSERT 或 INSERT INTO
        // - "Customers" / [Customers] / [dbo].[Customers]
        // - N'...' 或 '...'
        // - NULL
        // - 多行與空白
        var pattern = new Regex(
            @"INSERT\s+(?:INTO\s+)?(?:(?:\[dbo\]\.)?(?:\[Customers\]|""Customers""))\s*VALUES\s*\(\s*"
          + @"(?<id>(?:N)?'(?<idv>(?:''|[^'])*)'|NULL)\s*,\s*"
          + @"(?<company>(?:N)?'(?<companyv>(?:''|[^'])*)'|NULL)\s*,\s*"
          + @"(?<contact>(?:N)?'(?<contactv>(?:''|[^'])*)'|NULL)\s*,\s*"
          + @"(?<title>(?:N)?'(?<titlev>(?:''|[^'])*)'|NULL)\s*,\s*"
          + @"(?<address>(?:N)?'(?<addressv>(?:''|[^'])*)'|NULL)\s*,\s*"
          + @"(?<city>(?:N)?'(?<cityv>(?:''|[^'])*)'|NULL)\s*,\s*"
          + @"(?<region>(?:N)?'(?<regionv>(?:''|[^'])*)'|NULL)\s*,\s*"
          + @"(?<postal>(?:N)?'(?<postalv>(?:''|[^'])*)'|NULL)\s*,\s*"
          + @"(?<country>(?:N)?'(?<countryv>(?:''|[^'])*)'|NULL)\s*,\s*"
          + @"(?<phone>(?:N)?'(?<phonev>(?:''|[^'])*)'|NULL)\s*,\s*"
          + @"(?<fax>(?:N)?'(?<faxv>(?:''|[^'])*)'|NULL)\s*\)",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Multiline
        );

        static string? Unwrap(Group token, Group inner)
            => token.Value.Equals("NULL", StringComparison.OrdinalIgnoreCase)
               ? null
               : inner.Value.Replace("''", "'"); // 還原 SQL 轉義的單引號

        var matches = pattern.Matches(sqlText);
        if (matches.Count == 0)
            throw new InvalidOperationException("在 instnwnd.sql 中找不到符合 `INSERT \"Customers\" VALUES(...)` 的語句。");

        var customers = matches.Select(m => new Customer
        {
            Id = Unwrap(m.Groups["id"], m.Groups["idv"]) ?? throw new InvalidOperationException("CustomerID 不可為 NULL"),
            CompanyName = Unwrap(m.Groups["company"], m.Groups["companyv"]) ?? throw new InvalidOperationException("CompanyName 不可為 NULL"),
            ContactName = Unwrap(m.Groups["contact"], m.Groups["contactv"]),
            ContactTitle = Unwrap(m.Groups["title"], m.Groups["titlev"]),
            Address = Unwrap(m.Groups["address"], m.Groups["addressv"]),
            City = Unwrap(m.Groups["city"], m.Groups["cityv"]),
            Region = Unwrap(m.Groups["region"], m.Groups["regionv"]),
            PostalCode = Unwrap(m.Groups["postal"], m.Groups["postalv"]),
            Country = Unwrap(m.Groups["country"], m.Groups["countryv"]),
            Phone = Unwrap(m.Groups["phone"], m.Groups["phonev"]),
            Fax = Unwrap(m.Groups["fax"], m.Groups["faxv"]),
            CreatedBy = adminUserId,
            Created = now
        }).ToList();

        _context.Customers.AddRange(customers);
        await _context.SaveChangesAsync();
    }

    public async Task AddEmployeesAsync(string adminUserId, string sqlText)
    {
        if (_context.Employees.Any())
            return;

        var now = DateTimeOffset.UtcNow;

        // INSERT "Employees"("EmployeeID","LastName","FirstName","Title","TitleOfCourtesy",
        //                    "BirthDate","HireDate","Address","City","Region","PostalCode",
        //                    "Country","HomePhone","Extension","Photo","Notes","ReportsTo","PhotoPath")
        // VALUES (1,'Davolio','Nancy','Sales Representative','Ms.','12/08/1948','05/01/1992',
        //         '507 - 20th Ave. E.Apt. 2A','Seattle','WA','98122','USA','(206) 555-9857','5467',0x,
        //         '...notes...',2,'http://accweb/emmployees/davolio.bmp')
        var pattern = new Regex(
            @"INSERT\s+(?:INTO\s+)?(?:(?:\[dbo\]\.)?(?:\[Employees\]|""Employees""))\s*"
          + @"\(\s*(?:""EmployeeID""|\[EmployeeID\])\s*,\s*(?:""LastName""|\[LastName\])\s*,\s*(?:""FirstName""|\[FirstName\])\s*,\s*(?:""Title""|\[Title\])\s*,\s*(?:""TitleOfCourtesy""|\[TitleOfCourtesy\])\s*,\s*(?:""BirthDate""|\[BirthDate\])\s*,\s*(?:""HireDate""|\[HireDate\])\s*,\s*(?:""Address""|\[Address\])\s*,\s*(?:""City""|\[City\])\s*,\s*(?:""Region""|\[Region\])\s*,\s*(?:""PostalCode""|\[PostalCode\])\s*,\s*(?:""Country""|\[Country\])\s*,\s*(?:""HomePhone""|\[HomePhone\])\s*,\s*(?:""Extension""|\[Extension\])\s*,\s*(?:""Photo""|\[Photo\])\s*,\s*(?:""Notes""|\[Notes\])\s*,\s*(?:""ReportsTo""|\[ReportsTo\])\s*,\s*(?:""PhotoPath""|\[PhotoPath\])\s*\)\s*"
          + @"VALUES\s*\(\s*"
          + @"(?<id>\d+)\s*,\s*"
          + @"(?<last>(?:N)?'(?<lastv>(?:''|[^'])*)'|NULL)\s*,\s*"
          + @"(?<first>(?:N)?'(?<firstv>(?:''|[^'])*)'|NULL)\s*,\s*"
          + @"(?<title>(?:N)?'(?<titlev>(?:''|[^'])*)'|NULL)\s*,\s*"
          + @"(?<toc>(?:N)?'(?<tocv>(?:''|[^'])*)'|NULL)\s*,\s*"
          + @"(?<birth>(?:N)?'(?<birthv>(?:''|[^'])*)'|NULL)\s*,\s*"
          + @"(?<hire>(?:N)?'(?<hirev>(?:''|[^'])*)'|NULL)\s*,\s*"
          + @"(?<addr>(?:N)?'(?<addrv>(?:''|[^'])*)'|NULL)\s*,\s*"
          + @"(?<city>(?:N)?'(?<cityv>(?:''|[^'])*)'|NULL)\s*,\s*"
          + @"(?<region>(?:N)?'(?<regionv>(?:''|[^'])*)'|NULL)\s*,\s*"
          + @"(?<postal>(?:N)?'(?<postalv>(?:''|[^'])*)'|NULL)\s*,\s*"
          + @"(?<country>(?:N)?'(?<countryv>(?:''|[^'])*)'|NULL)\s*,\s*"
          + @"(?<home>(?:N)?'(?<homev>(?:''|[^'])*)'|NULL)\s*,\s*"
          + @"(?<ext>(?:N)?'(?<extv>(?:''|[^'])*)'|NULL)\s*,\s*"
          + @"(?<photo>0x[0-9A-Fa-f]*|NULL)\s*,\s*"
          + @"(?<notes>(?:N)?'(?<notesv>(?:''|[^'])*)'|NULL)\s*,\s*"
          + @"(?<reports>\d+|NULL)\s*,\s*"
          + @"(?<path>(?:N)?'(?<pathv>(?:''|[^'])*)'|NULL)\s*\)",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Multiline
        );

        static string? Unwrap(Group token, Group inner)
            => token.Value.Equals("NULL", StringComparison.OrdinalIgnoreCase)
               ? null
               : inner.Value.Replace("''", "'");

        static DateTime? ParseDateOrNull(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            // 支援幾種常見格式：MM/dd/yyyy、dd/MM/yyyy、yyyy-MM-dd 以及 M/d/yyyy
            string[] fmts = { "MM/dd/yyyy", "M/d/yyyy", "dd/MM/yyyy", "d/M/yyyy", "yyyy-MM-dd", "yyyy-MM-ddTHH:mm:ss", "yyyy-MM-dd HH:mm:ss" };
            if (DateTime.TryParseExact(s, fmts, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                return dt;
            // 退而求其次：一般 Parse（可能受系統文化影響）
            if (DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                return dt;
            return null;
        }

        static byte[]? HexToBytesOrNull(string token)
        {
            if (token.Equals("NULL", StringComparison.OrdinalIgnoreCase)) return null;
            var hex = token.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? token[2..] : token;
            if (hex.Length == 0) return Array.Empty<byte>(); // 支援 0x（空位元組）
            var bytes = new byte[hex.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
                bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            return bytes;
        }

        var matches = pattern.Matches(sqlText);
        if (matches.Count == 0)
            throw new InvalidOperationException("在 instnwnd.sql 中找不到 Employees 的 INSERT。");

        // 準備資料
        var employees = matches.Select(m =>
        {
            var id = int.Parse(m.Groups["id"].Value);
            var last = Unwrap(m.Groups["last"], m.Groups["lastv"]);
            var first = Unwrap(m.Groups["first"], m.Groups["firstv"]);

            return new Employee
            {
                Id = id, // 依腳本指定，需開 IDENTITY_INSERT
                LastName = last ?? throw new InvalidOperationException("LastName 不可為 NULL"),
                FirstName = first ?? throw new InvalidOperationException("FirstName 不可為 NULL"),
                Title = Unwrap(m.Groups["title"], m.Groups["titlev"]),
                TitleOfCourtesy = Unwrap(m.Groups["toc"], m.Groups["tocv"]),
                BirthDate = ParseDateOrNull(Unwrap(m.Groups["birth"], m.Groups["birthv"])),
                HireDate = ParseDateOrNull(Unwrap(m.Groups["hire"], m.Groups["hirev"])),
                Address = Unwrap(m.Groups["addr"], m.Groups["addrv"]),
                City = Unwrap(m.Groups["city"], m.Groups["cityv"]),
                Region = Unwrap(m.Groups["region"], m.Groups["regionv"]),
                PostalCode = Unwrap(m.Groups["postal"], m.Groups["postalv"]),
                Country = Unwrap(m.Groups["country"], m.Groups["countryv"]),
                HomePhone = Unwrap(m.Groups["home"], m.Groups["homev"]),
                Extension = Unwrap(m.Groups["ext"], m.Groups["extv"]),
                Photo = HexToBytesOrNull(m.Groups["photo"].Value),
                Notes = Unwrap(m.Groups["notes"], m.Groups["notesv"]) ?? string.Empty, // Northwind Notes 通常有值
                ReportsTo = m.Groups["reports"].Value.Equals("NULL", StringComparison.OrdinalIgnoreCase)
                            ? (int?)null : int.Parse(m.Groups["reports"].Value),
                PhotoPath = Unwrap(m.Groups["path"], m.Groups["pathv"]),
                CreatedBy = adminUserId,
                Created = now
            };
        }).OrderBy(e => e.Id).ToList();

        // 寫入（指定 EmployeeID -> 需開 IDENTITY_INSERT）
        using var tx = await _context.Database.BeginTransactionAsync();
        await _context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT [dbo].[Employees] ON");

        _context.Employees.AddRange(employees);
        await _context.SaveChangesAsync();

        await _context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT [dbo].[Employees] OFF");
        await tx.CommitAsync();
    }

    public async Task AddSuppliersAsync(string adminUserId, string sqlText)
    {
        if (_context.Suppliers.Any())
            return;

        var now = DateTimeOffset.UtcNow;
        var createdBy = string.IsNullOrWhiteSpace(adminUserId) ? "seed" : adminUserId;

        var pattern = new Regex(
            @"INSERT\s+(?:INTO\s+)?(?:(?:\[dbo\]\.)?(?:\[(?:Suppliers)\]|""Suppliers""))\s*\(\s*"
          + @"(?:""SupplierID""|\[SupplierID\])\s*,\s*(?:""CompanyName""|\[CompanyName\])\s*,\s*(?:""ContactName""|\[ContactName\])\s*,\s*(?:""ContactTitle""|\[ContactTitle\])\s*,\s*(?:""Address""|\[Address\])\s*,\s*(?:""City""|\[City\])\s*,\s*(?:""Region""|\[Region\])\s*,\s*(?:""PostalCode""|\[PostalCode\])\s*,\s*(?:""Country""|\[Country\])\s*,\s*(?:""Phone""|\[Phone\])\s*,\s*(?:""Fax""|\[Fax\])\s*,\s*(?:""HomePage""|\[HomePage\])\s*\)\s*"
          + @"VALUES\s*\(\s*"
          + @"(?<id>\d+)\s*,\s*"
          + @"(?<company>(?:N)?'(?<companyv>(?:''|[^'])*)'|NULL)\s*,\s*"
          + @"(?<contact>(?:N)?'(?<contactv>(?:''|[^'])*)'|NULL)\s*,\s*"
          + @"(?<title>(?:N)?'(?<titlev>(?:''|[^'])*)'|NULL)\s*,\s*"
          + @"(?<addr>(?:N)?'(?<addrv>(?:''|[^'])*)'|NULL)\s*,\s*"
          + @"(?<city>(?:N)?'(?<cityv>(?:''|[^'])*)'|NULL)\s*,\s*"
          + @"(?<region>(?:N)?'(?<regionv>(?:''|[^'])*)'|NULL)\s*,\s*"
          + @"(?<postal>(?:N)?'(?<postalv>(?:''|[^'])*)'|NULL)\s*,\s*"
          + @"(?<country>(?:N)?'(?<countryv>(?:''|[^'])*)'|NULL)\s*,\s*"
          + @"(?<phone>(?:N)?'(?<phonev>(?:''|[^'])*)'|NULL)\s*,\s*"
          + @"(?<fax>(?:N)?'(?<faxv>(?:''|[^'])*)'|NULL)\s*,\s*"
          + @"(?<home>(?:N)?'(?<homev>(?:''|[^'])*)'|NULL)\s*\)",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Multiline);

        MatchCollection matches = pattern.Matches(sqlText);

        if (matches.Count == 0)
            throw new InvalidOperationException("在 instnwnd.sql 中找不到 Suppliers 的 INSERT。");

        static string? Unwrap(Group token, Group inner)
            => token.Value.Equals("NULL", StringComparison.OrdinalIgnoreCase)
               ? null
               : inner.Value.Replace("''", "'");

        var suppliers = matches.Select(m => new Supplier
        {
            Id = int.Parse(m.Groups["id"].Value, CultureInfo.InvariantCulture), // 依腳本指定
            CompanyName = Unwrap(m.Groups["company"], m.Groups["companyv"]) ?? throw new InvalidOperationException("CompanyName 不可為 NULL"),
            ContactName = Unwrap(m.Groups["contact"], m.Groups["contactv"]),
            ContactTitle = Unwrap(m.Groups["title"], m.Groups["titlev"]),
            Address = Unwrap(m.Groups["addr"], m.Groups["addrv"]),
            City = Unwrap(m.Groups["city"], m.Groups["cityv"]),
            Region = Unwrap(m.Groups["region"], m.Groups["regionv"]),
            PostalCode = Unwrap(m.Groups["postal"], m.Groups["postalv"]),
            Country = Unwrap(m.Groups["country"], m.Groups["countryv"]),
            Phone = Unwrap(m.Groups["phone"], m.Groups["phonev"]),
            Fax = Unwrap(m.Groups["fax"], m.Groups["faxv"]),
            HomePage = Unwrap(m.Groups["home"], m.Groups["homev"]),
            Created = now,
            CreatedBy = createdBy
        })
        .GroupBy(s => s.Id)
        .Select(g => g.First())
        .OrderBy(s => s.Id)
        .ToList();

        using var tx = await _context.Database.BeginTransactionAsync();
        await _context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT [dbo].[Suppliers] ON");

        _context.Suppliers.AddRange(suppliers);
        await _context.SaveChangesAsync();

        await _context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT [dbo].[Suppliers] OFF");
        await tx.CommitAsync();
    }

    public async Task AddProductsAsync(string adminUserId, string sqlText)
    {
        if (_context.Products.Any())
            return;

        var now = DateTimeOffset.UtcNow;
        var createdBy = string.IsNullOrWhiteSpace(adminUserId) ? "seed" : adminUserId;

        var pattern = new Regex(
            @"INSERT\s+(?:INTO\s+)?(?:(?:\[dbo\]\.)?(?:\[(?:Products)\]|""Products""))\s*\(\s*"
          + @"(?:""ProductID""|\[ProductID\])\s*,\s*(?:""ProductName""|\[ProductName\])\s*,\s*(?:""SupplierID""|\[SupplierID\])\s*,\s*(?:""CategoryID""|\[CategoryID\])\s*,\s*(?:""QuantityPerUnit""|\[QuantityPerUnit\])\s*,\s*(?:""UnitPrice""|\[UnitPrice\])\s*,\s*(?:""UnitsInStock""|\[UnitsInStock\])\s*,\s*(?:""UnitsOnOrder""|\[UnitsOnOrder\])\s*,\s*(?:""ReorderLevel""|\[ReorderLevel\])\s*,\s*(?:""Discontinued""|\[Discontinued\])\s*\)\s*"
          + @"VALUES\s*\(\s*"
          + @"(?<id>\d+)\s*,\s*"
          + @"(?<name>(?:N)?'(?<namev>(?:''|[^'])*)'|NULL)\s*,\s*"
          + @"(?<supplier>\d+|NULL)\s*,\s*"
          + @"(?<category>\d+|NULL)\s*,\s*"
          + @"(?<qpu>(?:N)?'(?<qpuv>(?:''|[^'])*)'|NULL)\s*,\s*"
          + @"(?<price>-?\d+(?:\.\d+)?|NULL)\s*,\s*"
          + @"(?<stock>-?\d+|NULL)\s*,\s*"
          + @"(?<onorder>-?\d+|NULL)\s*,\s*"
          + @"(?<reorder>-?\d+|NULL)\s*,\s*"
          + @"(?<disc>-?\d+|NULL)\s*\)",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Multiline);

        MatchCollection matches = pattern.Matches(sqlText);

        if (matches.Count == 0)
            throw new InvalidOperationException("在 instnwnd.sql 中找不到 Products 的 INSERT。");

        static string? Unwrap(Group token, Group inner)
            => token.Value.Equals("NULL", StringComparison.OrdinalIgnoreCase)
               ? null
               : inner.Value.Replace("''", "'");

        static int? ToIntOrNull(string s)
            => s.Equals("NULL", StringComparison.OrdinalIgnoreCase) ? (int?)null : int.Parse(s, CultureInfo.InvariantCulture);

        static short? ToShortOrNull(string s)
            => s.Equals("NULL", StringComparison.OrdinalIgnoreCase) ? (short?)null : (short)int.Parse(s, CultureInfo.InvariantCulture);

        static decimal? ToDecimalOrNull(string s)
            => s.Equals("NULL", StringComparison.OrdinalIgnoreCase) ? (decimal?)null : decimal.Parse(s, NumberStyles.Number | NumberStyles.AllowExponent, CultureInfo.InvariantCulture);

        static bool ToBoolFromBit(string s)
            => !s.Equals("0", StringComparison.OrdinalIgnoreCase) && !s.Equals("NULL", StringComparison.OrdinalIgnoreCase);

        var products = matches.Select(m => new Product
        {
            Id = int.Parse(m.Groups["id"].Value, CultureInfo.InvariantCulture), // 依腳本指定
            ProductName = Unwrap(m.Groups["name"], m.Groups["namev"]) ?? throw new InvalidOperationException("ProductName 不可為 NULL"),
            SupplierId = ToIntOrNull(m.Groups["supplier"].Value),
            CategoryId = ToIntOrNull(m.Groups["category"].Value),
            QuantityPerUnit = Unwrap(m.Groups["qpu"], m.Groups["qpuv"]),
            UnitPrice = ToDecimalOrNull(m.Groups["price"].Value),
            UnitsInStock = ToShortOrNull(m.Groups["stock"].Value),
            UnitsOnOrder = ToShortOrNull(m.Groups["onorder"].Value),
            ReorderLevel = ToShortOrNull(m.Groups["reorder"].Value),
            Discontinued = ToBoolFromBit(m.Groups["disc"].Value),
            Created = now,
            CreatedBy = createdBy
        })
        // 去重（避免同一檔有重複 INSERT）
        .GroupBy(p => p.Id)
        .Select(g => g.First())
        .OrderBy(p => p.Id)
        .ToList();

        using var tx = await _context.Database.BeginTransactionAsync();
        await _context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT [dbo].[Products] ON");

        _context.Products.AddRange(products);
        await _context.SaveChangesAsync();

        await _context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT [dbo].[Products] OFF");
        await tx.CommitAsync();
    }

    public async Task AddShippersAsync(string adminUserId, string sqlText)
    {
        if (_context.Shippers.Any())
            return;

        var now = DateTimeOffset.UtcNow;
        var createdBy = string.IsNullOrWhiteSpace(adminUserId) ? "seed" : adminUserId;

        var pattern = new Regex(
            @"INSERT\s+(?:INTO\s+)?(?:(?:\[dbo\]\.)?(?:\[(?:Shippers)\]|""Shippers""))\s*\(\s*"
          + @"(?:""ShipperID""|\[ShipperID\])\s*,\s*(?:""CompanyName""|\[CompanyName\])\s*,\s*(?:""Phone""|\[Phone\])\s*\)\s*"
          + @"VALUES\s*\(\s*(?<id>\d+)\s*,\s*(?<company>(?:N)?'(?<companyv>(?:''|[^'])*)'|NULL)\s*,\s*(?<phone>(?:N)?'(?<phonev>(?:''|[^'])*)'|NULL)\s*\)",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Multiline);

        MatchCollection matches = pattern.Matches(sqlText);

        if (matches.Count == 0)
            throw new InvalidOperationException("在 instnwnd.sql 中找不到 Shippers 的 INSERT。");

        static string? Unwrap(Group token, Group inner)
            => token.Value.Equals("NULL", StringComparison.OrdinalIgnoreCase)
               ? null
               : inner.Value.Replace("''", "'");

        var shippers = matches.Select(m => new Shipper
        {
            Id = int.Parse(m.Groups["id"].Value, CultureInfo.InvariantCulture), // 指定腳本中的 ShipperID
            CompanyName = Unwrap(m.Groups["company"], m.Groups["companyv"])
                          ?? throw new InvalidOperationException("CompanyName 不可為 NULL"),
            Phone = Unwrap(m.Groups["phone"], m.Groups["phonev"]),
        })
        .GroupBy(s => s.Id)     // 避免腳本重複
        .Select(g => g.First())
        .OrderBy(s => s.Id)
        .ToList();

        using var tx = await _context.Database.BeginTransactionAsync();
        await _context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT [dbo].[Shippers] ON");

        _context.Shippers.AddRange(shippers);
        await _context.SaveChangesAsync();

        await _context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT [dbo].[Shippers] OFF");
        await tx.CommitAsync();
    }

    public async Task AddOrdersAsync(string adminUserId, string sqlText)
    {
        if (_context.Orders.Any())
            return;

        var now = DateTimeOffset.UtcNow;
        var createdBy = string.IsNullOrWhiteSpace(adminUserId) ? "seed" : adminUserId;

        var pattern = new Regex(
            @"INSERT\s+(?:INTO\s+)?(?:(?:\[dbo\]\.)?(?:\[(?:Orders)\]|""Orders""))\s*\(\s*"
          + @"(?:""OrderID""|\[OrderID\])\s*,\s*(?:""CustomerID""|\[CustomerID\])\s*,\s*(?:""EmployeeID""|\[EmployeeID\])\s*,\s*(?:""OrderDate""|\[OrderDate\])\s*,\s*(?:""RequiredDate""|\[RequiredDate\])\s*,\s*(?:""ShippedDate""|\[ShippedDate\])\s*,\s*(?:""ShipVia""|\[ShipVia\])\s*,\s*(?:""Freight""|\[Freight\])\s*,\s*(?:""ShipName""|\[ShipName\])\s*,\s*(?:""ShipAddress""|\[ShipAddress\])\s*,\s*(?:""ShipCity""|\[ShipCity\])\s*,\s*(?:""ShipRegion""|\[ShipRegion\])\s*,\s*(?:""ShipPostalCode""|\[ShipPostalCode\])\s*,\s*(?:""ShipCountry""|\[ShipCountry\])\s*\)\s*"
          + @"VALUES\s*\(\s*"
          + @"(?<id>\d+)\s*,\s*"
          + @"(?<cust>(?:N)?'(?<custv>(?:''|[^'])*)'|NULL)\s*,\s*"
          + @"(?<emp>\d+|NULL)\s*,\s*"
          + @"(?<od>(?:N)?'(?<odv>(?:''|[^'])*)'|NULL)\s*,\s*"
          + @"(?<req>(?:N)?'(?<reqv>(?:''|[^'])*)'|NULL)\s*,\s*"
          + @"(?<shipd>(?:N)?'(?<shipdv>(?:''|[^'])*)'|NULL)\s*,\s*"
          + @"(?<shipvia>\d+|NULL)\s*,\s*"
          + @"(?<freight>-?\d+(?:\.\d+)?|NULL)\s*,\s*"
          + @"(?<shipname>(?:N)?'(?<shipnamev>(?:''|[^'])*)'|NULL)\s*,\s*"
          + @"(?<shipaddr>(?:N)?'(?<shipaddrv>(?:''|[^'])*)'|NULL)\s*,\s*"
          + @"(?<shipcity>(?:N)?'(?<shipcityv>(?:''|[^'])*)'|NULL)\s*,\s*"
          + @"(?<shipregion>(?:N)?'(?<shipregionv>(?:''|[^'])*)'|NULL)\s*,\s*"
          + @"(?<shippostal>(?:N)?'(?<shippostalv>(?:''|[^'])*)'|NULL)\s*,\s*"
          + @"(?<shipcountry>(?:N)?'(?<shipcountryv>(?:''|[^'])*)'|NULL)\s*\)",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Multiline);

        MatchCollection matches = pattern.Matches(sqlText);
        if (matches.Count == 0)
            throw new InvalidOperationException("在 instnwnd.sql 中找不到 Orders 的 INSERT。");

        static string? Unwrap(Group token, Group inner)
            => token.Value.Equals("NULL", StringComparison.OrdinalIgnoreCase)
               ? null
               : inner.Value.Replace("''", "'");

        static int? ToIntOrNull(string s)
            => s.Equals("NULL", StringComparison.OrdinalIgnoreCase) ? (int?)null : int.Parse(s, CultureInfo.InvariantCulture);

        static decimal? ToDecimalOrNull(string s)
            => s.Equals("NULL", StringComparison.OrdinalIgnoreCase) ? (decimal?)null
               : decimal.Parse(s, NumberStyles.Number | NumberStyles.AllowExponent, CultureInfo.InvariantCulture);

        static DateTime? ParseDateOrNull(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            // 常見格式（含美式/歐式/ISO）
            string[] fmts = {
            "MM/dd/yyyy", "M/d/yyyy", "dd/MM/yyyy", "d/M/yyyy",
            "yyyy-MM-dd", "yyyy-MM-dd HH:mm:ss", "yyyy-MM-ddTHH:mm:ss"
        };
            if (DateTime.TryParseExact(s, fmts, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                return dt;
            // 退一步用一般 Parse（盡量用 Invariant）
            if (DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                return dt;
            return null;
        }

        var orders = new List<Order>();
        foreach (Match m in matches)
        {
            orders.Add(new Order
            {
                Id = int.Parse(m.Groups["id"].Value, CultureInfo.InvariantCulture),
                CustomerId = Unwrap(m.Groups["cust"], m.Groups["custv"]),
                DepartmentId = 1,
                OfficeId = RandomNumberGenerator.GetInt32(1, 5),
                EmployeeId = ToIntOrNull(m.Groups["emp"].Value),
                OrderDate = ParseDateOrNull(Unwrap(m.Groups["od"], m.Groups["odv"])),
                RequiredDate = ParseDateOrNull(Unwrap(m.Groups["req"], m.Groups["reqv"])),
                ShippedDate = ParseDateOrNull(Unwrap(m.Groups["shipd"], m.Groups["shipdv"])),
                ShipVia = ToIntOrNull(m.Groups["shipvia"].Value),
                Freight = ToDecimalOrNull(m.Groups["freight"].Value),
                ShipName = Unwrap(m.Groups["shipname"], m.Groups["shipnamev"]),
                ShipAddress = Unwrap(m.Groups["shipaddr"], m.Groups["shipaddrv"]),
                ShipCity = Unwrap(m.Groups["shipcity"], m.Groups["shipcityv"]),
                ShipRegion = Unwrap(m.Groups["shipregion"], m.Groups["shipregionv"]),
                ShipPostalCode = Unwrap(m.Groups["shippostal"], m.Groups["shippostalv"]),
                ShipCountry = Unwrap(m.Groups["shipcountry"], m.Groups["shipcountryv"]),
                Created = now,
                CreatedBy = createdBy
            });
        }

        orders = orders
            .GroupBy(o => o.Id)       // 檔案若不小心重複同一 OrderID，只取第一筆
            .Select(g => g.First())
            .OrderBy(o => o.Id)
            .ToList();

        //var orders = matches.Select(m => new Order
        //{
        //    Id = int.Parse(m.Groups["id"].Value, CultureInfo.InvariantCulture),
        //    CustomerId = Unwrap(m.Groups["cust"], m.Groups["custv"]),
        //    DepartmentId = 1,
        //    OfficeId = 1,
        //    EmployeeId = ToIntOrNull(m.Groups["emp"].Value),
        //    OrderDate = ParseDateOrNull(Unwrap(m.Groups["od"], m.Groups["odv"])),
        //    RequiredDate = ParseDateOrNull(Unwrap(m.Groups["req"], m.Groups["reqv"])),
        //    ShippedDate = ParseDateOrNull(Unwrap(m.Groups["shipd"], m.Groups["shipdv"])),
        //    ShipVia = ToIntOrNull(m.Groups["shipvia"].Value),
        //    Freight = ToDecimalOrNull(m.Groups["freight"].Value),
        //    ShipName = Unwrap(m.Groups["shipname"], m.Groups["shipnamev"]),
        //    ShipAddress = Unwrap(m.Groups["shipaddr"], m.Groups["shipaddrv"]),
        //    ShipCity = Unwrap(m.Groups["shipcity"], m.Groups["shipcityv"]),
        //    ShipRegion = Unwrap(m.Groups["shipregion"], m.Groups["shipregionv"]),
        //    ShipPostalCode = Unwrap(m.Groups["shippostal"], m.Groups["shippostalv"]),
        //    ShipCountry = Unwrap(m.Groups["shipcountry"], m.Groups["shipcountryv"]),
        //    Created = now,
        //    CreatedBy = createdBy
        //})
        //.GroupBy(o => o.Id)       // 檔案若不小心重複同一 OrderID，只取第一筆
        //.Select(g => g.First())
        //.OrderBy(o => o.Id)
        //.ToList();

        using var tx = await _context.Database.BeginTransactionAsync();
        await _context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT [dbo].[Orders] ON");

        _context.Orders.AddRange(orders);
        await _context.SaveChangesAsync();

        await _context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT [dbo].[Orders] OFF");
        await tx.CommitAsync();
    }

    public async Task AddOrderDetailsAsync(string adminUserId, string sqlText)
    {
        if (_context.OrderDetails.Any())
            return;

        var now = DateTimeOffset.UtcNow;

        var pattern = new Regex(
            @"INSERT\s+(?:INTO\s+)?(?:(?:\[dbo\]\.)?(?:\[(?:Order Details)\]|""Order Details""))\s*VALUES\s*\(\s*"
          + @"(?<order>\d+)\s*,\s*(?<product>\d+)\s*,\s*(?<price>-?\d+(?:\.\d+)?)\s*,\s*(?<qty>\d+)\s*,\s*(?<disc>-?\d+(?:\.\d+)?)\s*\)",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Multiline);

        MatchCollection matches = pattern.Matches(sqlText);
        if (matches.Count == 0)
            throw new InvalidOperationException("在 instnwnd.sql 中找不到 Order Details 的 INSERT。");

        static decimal ParseDecimal(string s) =>
            decimal.Parse(s, NumberStyles.Number | NumberStyles.AllowExponent, CultureInfo.InvariantCulture);

        static float ParseFloat(string s) =>
            float.Parse(s, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture);

        var items = matches.Select(m => new OrderDetail
        {
            OrderId = int.Parse(m.Groups["order"].Value, CultureInfo.InvariantCulture),
            ProductId = int.Parse(m.Groups["product"].Value, CultureInfo.InvariantCulture),
            UnitPrice = ParseDecimal(m.Groups["price"].Value),          // money -> decimal
            Quantity = (short)int.Parse(m.Groups["qty"].Value, CultureInfo.InvariantCulture),
            Discount = ParseFloat(m.Groups["disc"].Value),             // real -> float (0~1)
        })
        // 保險：排序一下，且若有重複對（OrderId,ProductId）就只留一筆
        .GroupBy(x => new { x.OrderId, x.ProductId })
        .Select(g => g.First())
        .OrderBy(x => x.OrderId).ThenBy(x => x.ProductId)
        .ToList();

        using var tx = await _context.Database.BeginTransactionAsync();
        await _context.Database.ExecuteSqlRawAsync("ALTER TABLE OrderDetails NOCHECK CONSTRAINT ALL");

        _context.OrderDetails.AddRange(items);
        await _context.SaveChangesAsync();

        await _context.Database.ExecuteSqlRawAsync("ALTER TABLE OrderDetails CHECK CONSTRAINT ALL");
        await tx.CommitAsync();
    }

    public async Task AddRegionsAsync(string adminUserId, string sqlText)
    {
        if (_context.Regions.Any())
            return;

        var now = DateTimeOffset.UtcNow;

        var pattern = new Regex(
            @"INSERT\s+(?:INTO\s+)?(?:" +
                @"(?:(?:\[dbo\]\.)?(?:\[Region\]|""Region""))" + // [Region] 或 "Region"（可含 dbo.）
                @"|(?:dbo\.)?Region" +                            // 裸表名 Region（可含 dbo.）
            @")\s*VALUES\s*\(\s*" +
            @"(?<id>\d+)\s*,\s*(?<desc>(?:N)?'(?<descv>(?:''|[^'])*)'|NULL)\s*\)",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Multiline);

        MatchCollection matches = pattern.Matches(sqlText);
        if (matches.Count == 0)
            throw new InvalidOperationException("在 instnwnd.sql 中找不到 Region 的 INSERT。");

        static string? Unwrap(Group token, Group inner)
            => token.Value.Equals("NULL", StringComparison.OrdinalIgnoreCase)
               ? null
               : inner.Value.Replace("''", "'");

        var regions = matches.Select(m => new Region
        {
            Id = int.Parse(m.Groups["id"].Value, CultureInfo.InvariantCulture),
            RegionDescription = Unwrap(m.Groups["desc"], m.Groups["descv"])
                                ?? throw new InvalidOperationException("RegionDescription 不可為 NULL"),
        })
        .GroupBy(r => r.Id)      // 去重
        .Select(g => g.First())
        .OrderBy(r => r.Id)
        .ToList();

        _context.Regions.AddRange(regions);
        await _context.SaveChangesAsync();
    }

    public async Task AddEmployeeTerritoriesAsync(string adminUserId, string sqlText)
    {
        if (_context.EmployeeTerritories.Any())
            return;

        var now = DateTimeOffset.UtcNow;

        var pattern = new Regex(
            @"INSERT\s+(?:INTO\s+)?(?:" +
                @"(?:(?:\[dbo\]\.)?(?:\[EmployeeTerritories\]|""EmployeeTerritories""))" + // [dbo].[EmployeeTerritories] / "EmployeeTerritories"
                @"|(?:dbo\.)?EmployeeTerritories" +                                        // 裸表名（可含 dbo.）
            @")\s*VALUES\s*\(\s*" +
            @"(?<emp>\d+)\s*,\s*(?<terr>(?:N)?'(?<terrv>(?:''|[^'])*)'|\d+)\s*\)",        // TerritoryID 可能是 '06897' 或 06897（雖然型別是字串）
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Multiline);

        var matches = pattern.Matches(sqlText);
        if (matches.Count == 0)
            throw new InvalidOperationException("在 instnwnd.sql 中找不到 EmployeeTerritories 的 INSERT。");

        static string UnwrapTerritoryId(Group token, Group inner)
        {
            // 若有引號，還原 '' -> '
            if (inner.Success) return inner.Value.Replace("''", "'");
            // 若是數字沒引號，直接回傳原字串
            return token.Value;
        }

        var rows = matches.Select(m => new EmployeeTerritory
        {
            EmployeeId = int.Parse(m.Groups["emp"].Value, CultureInfo.InvariantCulture),
            TerritoryId = UnwrapTerritoryId(m.Groups["terr"], m.Groups["terrv"]),
        })
        // 去重：避免同一對 (EmployeeId, TerritoryId) 重複
        .GroupBy(x => new { x.EmployeeId, x.TerritoryId })
        .Select(g => g.First())
        .OrderBy(x => x.EmployeeId).ThenBy(x => x.TerritoryId)
        .ToList();

        _context.EmployeeTerritories.AddRange(rows);
        await _context.SaveChangesAsync();
    }

    public async Task AddTerritoriesAsync(string adminUserId, string sqlText)
    {
        if (_context.Territories.Any())
            return;

        var now = DateTimeOffset.UtcNow;

        var pattern = new Regex(
            @"INSERT\s+(?:INTO\s+)?(?:" +
                @"(?:(?:\[dbo\]\.)?(?:\[(?:Terrories|Territories)\]|""Territories""))" + // 引號/中括號/含 dbo.
                @"|(?:dbo\.)?Territories" +                                             // 裸表名（含可選 dbo.）
            @")\s*VALUES\s*\(\s*" +
            @"(?<id>(?:N)?'(?<idv>(?:''|[^'])*)'|\d+|NULL)\s*,\s*" +     // TerritoryID: 字串或數字或 NULL
            @"(?<desc>(?:N)?'(?<descv>(?:''|[^'])*)'|NULL)\s*,\s*" +     // TerritoryDescription: 字串或 NULL
            @"(?<region>\d+|NULL)\s*\)\s*;?",                            // RegionID: 整數或 NULL
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Multiline);

        var matches = pattern.Matches(sqlText);
        if (matches.Count == 0)
            throw new InvalidOperationException("在 instnwnd.sql 中找不到 Territories 的 INSERT（含裸表名）。");

        static string? UnwrapQuotedOrNull(Group token, Group inner)
            => token.Value.Equals("NULL", StringComparison.OrdinalIgnoreCase)
               ? null
               : inner.Success ? inner.Value.Replace("''", "'") : token.Value;

        static int? ToIntOrNull(string s)
            => s.Equals("NULL", StringComparison.OrdinalIgnoreCase) ? (int?)null : int.Parse(s, CultureInfo.InvariantCulture);

        var territories = matches.Select(m =>
        {
            var idRaw = UnwrapQuotedOrNull(m.Groups["id"], m.Groups["idv"]);
            var desc = UnwrapQuotedOrNull(m.Groups["desc"], m.Groups["descv"]);
            var region = ToIntOrNull(m.Groups["region"].Value);

            return new Territory
            {
                Id = idRaw ?? throw new InvalidOperationException("TerritoryID 不可為 NULL"),
                TerritoryDescription = desc ?? throw new InvalidOperationException("TerritoryDescription 不可為 NULL"),
                RegionId = region ?? throw new InvalidOperationException("RegionID 不可為 NULL"),
            };
        })
        .GroupBy(t => t.Id)
        .Select(g => g.First())
        .OrderBy(t => t.Id)
        .ToList();

        _context.Territories.AddRange(territories);
        await _context.SaveChangesAsync();
    }

    #endregion

    public static string FindInstNwnd(string relative = "sql/instnwnd.sql")
    {
        // 1) 先試 bin\...\sql\instnwnd.sql
        var pathInBin = Path.Combine(AppContext.BaseDirectory, relative);
        if (File.Exists(pathInBin)) return pathInBin;

        // 2) 往上找，直到根目錄
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            var candidate = Path.Combine(dir.FullName, relative);
            if (File.Exists(candidate)) return candidate;

            // 可選：遇到 .sln 視為方案根目錄，再檢一次
            if (dir.GetFiles("*.sln").Any())
            {
                var atSolutionRoot = Path.Combine(dir.FullName, relative);
                if (File.Exists(atSolutionRoot)) return atSolutionRoot;
            }

            dir = dir.Parent;
        }

        throw new FileNotFoundException($"找不到 {relative}，請確認路徑。起點：{AppContext.BaseDirectory}");
    }
}
