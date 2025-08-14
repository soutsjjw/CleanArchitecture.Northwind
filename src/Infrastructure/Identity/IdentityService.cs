using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text;
using CleanArchitecture.Northwind.Application.Common.Exceptions;
using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Common.Interfaces.Identity;
using CleanArchitecture.Northwind.Application.Common.Logging;
using CleanArchitecture.Northwind.Application.Common.Models;
using CleanArchitecture.Northwind.Application.Common.Models.Letter;
using CleanArchitecture.Northwind.Domain.Entities.Identity;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace CleanArchitecture.Northwind.Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly IApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IUserStore<ApplicationUser> _userStore;

    private readonly IJwtTokenService _jwtTokenService;
    private readonly IUserClaimsPrincipalFactory<ApplicationUser> _userClaimsPrincipalFactory;
    private readonly IAuthorizationService _authorizationService;
    private readonly IMailService _mailService;
    private readonly IDataProtectionService _dataProtectionService;

    private static readonly EmailAddressAttribute _emailAddressAttribute = new();

    private readonly ILogger<IdentityService> _logger;
    private readonly IAppConfigurationSettings _appConfig;
    private readonly IIdentitySettings _identitySettings;

    public IdentityService(
        IApplicationDbContext context,
        IConfiguration configuration,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        RoleManager<ApplicationRole> roleManager,
        IUserStore<ApplicationUser> userStore,
        IJwtTokenService jwtTokenService,
        IUserClaimsPrincipalFactory<ApplicationUser> userClaimsPrincipalFactory,
        IAuthorizationService authorizationService,
        IMailService mailService,
        IDataProtectionService dataProtectionService,
        ILogger<IdentityService> logger,
        IAppConfigurationSettings appConfig,
        IIdentitySettings identitySettings)
    {
        _context = context;
        _configuration = configuration;
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _userStore = userStore;

        _jwtTokenService = jwtTokenService;

        _userClaimsPrincipalFactory = userClaimsPrincipalFactory;
        _authorizationService = authorizationService;
        _mailService = mailService;
        _dataProtectionService = dataProtectionService;

        _logger = logger;
        _appConfig = appConfig;
        _identitySettings = identitySettings;
    }

    public async Task<ApplicationUser?> GetUserByIdAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            _logger.LogWarning(LoggingEvents.Account.UserNotFoundFormat, userId);
            return null;
        }

        var profile = await _context.UserProfiles
            .Where(x => x.UserId == user.Id)
            .FirstOrDefaultAsync();

        if (profile == null)
        {
            _logger.LogWarning(LoggingEvents.Account.UserProfileNotFoundFormat, userId);
            return user;
        }

        user.Profile = profile;

        return user;
    }

    public async Task<ApplicationUser?> GetUserByEmailAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            _logger.LogWarning(LoggingEvents.Account.UserNotFoundFormat, email);
            return null;
        }

        return await GetUserAsync(user);
    }

    private async Task<ApplicationUser?> GetUserAsync(ApplicationUser user)
    {
        var profile = await _context.UserProfiles
            .Where(x => x.UserId == user.Id)
            .FirstOrDefaultAsync();

        if (profile == null)
        {
            _logger.LogWarning(LoggingEvents.Account.UserProfileNotFoundFormat, user.Id);
            return user;
        }

        var passwordHistories = _context.UserPasswordHistories
            .Where(x => x.UserId == user.Id)
            .ToList();

        user.Profile = profile;
        user.PasswordHistories = passwordHistories;

        return user;
    }

    public async Task<string> UserRegisterAsync(string userName, string password)
    {
        if (!_userManager.SupportsUserEmail)
        {
            throw new BadRequestException("需要具有電子郵件支援的使用者儲存");
        }

        var emailStore = (IUserEmailStore<ApplicationUser>)_userStore;
        var email = userName;

        if (string.IsNullOrEmpty(email) || !_emailAddressAttribute.IsValid(email))
        {
            throw new BadRequestException(IdentityResult.Failed(_userManager.ErrorDescriber.InvalidEmail(email)).ToString());
        }

        if ((await _userManager.FindByEmailAsync(email)) != null)
        {
            _logger.LogError("帳號 {Email} 重複註冊", email);

            throw new ArgumentException("帳號註冊失敗");
        }

        var user = new ApplicationUser();
        // 設置或更改帳號名稱
        await _userStore.SetUserNameAsync(user, email, CancellationToken.None);
        // 設置或更改帳號的電子郵件地址
        await emailStore.SetEmailAsync(user, email, CancellationToken.None);
        // CreateAsync 方法可以建立帳號，但並不會自動設置帳號名稱和電子郵件地址
        var result = await _userManager.CreateAsync(user, password);

        if (!result.Succeeded)
        {
            throw new BadRequestException(result.ToString());
        }

        return user.Id;
    }

    public async Task<string> GenerateEmailConfirmationTokenAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            throw new UnauthorizedException("未找到使用者");
        }

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

        return token;
    }

    public async Task<(SignInResult? Result, ApplicationUser? User)> UserLogin(string userName, string password, bool useCookies)
    {
        var (result, user) = await ValidateUserAsync(userName, password);

        if (user == null)
        {
            return (SignInResult.Failed, null);
        }

        if (!result.Succeeded)
        {
            return (result, user);
        }

        user.Profile = await _context.UserProfiles.Where(x => x.UserId == user.Id).FirstOrDefaultAsync();

        if (user.Profile == null)
        {
            _logger.LogWarning(LoggingEvents.Account.UserProfileNotFoundFormat, user.Id);
            return (SignInResult.Failed, user);
        }

        /*
        if (useCookies)
        {
            result = await _signInManager.PasswordSignInAsync(user, password, isPersistent: false, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                var claims = new List<Claim>
                {
                    new Claim("FullName", user.Profile?.FullName ?? ""),
                    new Claim("IDNo", user.Profile?.IDNo ?? ""),
                    new Claim("Gender", user.Profile?.Gender.ToString() ?? nameof(Domain.Enums.Gender.Unknow)),
                    new Claim("Title", user.Profile?.Title ?? ""),
                    new Claim("Status", user.Profile?.Status.ToString() ?? nameof(Domain.Enums.Status.Disable)),
                };

                // 添加聲明到 ClaimsIdentity
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                // 登入使用者並附加聲明
                await _signInManager.SignInWithClaimsAsync(user, isPersistent: false, additionalClaims: claims);
            }

            isPasswordValid = result.Succeeded;
        }
        else
        {
            isPasswordValid = await _userManager.CheckPasswordAsync(user, password);
        }
        */

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, password);

        if (isPasswordValid)
        {
            if (!_identitySettings.ForceEnableTotp && !user.Profile.IsTotpEnabled)
            {
                await SignInAsync(user, useCookies);
            }
        }
        else
        {
            _logger.LogWarning(LoggingEvents.Account.InvalidLoginAttemptFormat, userName);

            return (SignInResult.Failed, user);
        }

        return (SignInResult.Success, user);
    }

    public async Task<(SignInResult, AccessTokenResponse? token)> UserLoginByAPI(string userName, string password)
    {
        var (result, user) = await ValidateUserAsync(userName, password);

        if (user == null)
        {
            return (SignInResult.Failed, null);
        }

        if (!result.Succeeded)
        {
            return (result, null);
        }

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, password);

        if (!isPasswordValid)
        {
            _logger.LogWarning(LoggingEvents.Account.InvalidLoginAttemptFormat, userName);

            return (SignInResult.Failed, null);
        }

        return (SignInResult.Success, await GenerateTokenResponseAsync(user));
    }

    public async Task SignInAsync(ApplicationUser user, bool useCookies)
    {
        await _signInManager.SignInAsync(user, false);

        if (useCookies)
        {
            var claims = new List<Claim>
                {
                    new Claim("FullName", user.Profile?.FullName ?? ""),
                    new Claim("IDNo", user.Profile?.IDNo ?? ""),
                    new Claim("Gender", user.Profile?.Gender.ToString() ?? nameof(Domain.Enums.Gender.Unknow)),
                    new Claim("Title", user.Profile?.Title ?? ""),
                    new Claim("Status", user.Profile?.Status.ToString() ?? nameof(Domain.Enums.Status.Disable)),
                    new Claim("OfficeId", user.Profile?.OfficeId.ToString()),
                    new Claim("DepartmentId", user.Profile?.DepartmentId.ToString())
                };

            // 添加聲明到 ClaimsIdentity
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            // 登入使用者並附加聲明
            await _signInManager.SignInWithClaimsAsync(user, isPersistent: false, additionalClaims: claims);
        }
    }

    public async Task<AccessTokenResponse> RefreshByAPI(string refreshToken)
    {
        ClaimsPrincipal userPrincipal = null;
        string userId = string.Empty;

        try
        {
            userPrincipal = _jwtTokenService.GetPrincipalFromExpiredToken(refreshToken);
            userId = userPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
        }
        catch (SecurityTokenSignatureKeyNotFoundException ex)
        {
            throw new UnauthorizedException(LoggingEvents.Account.InvalidClientToken);
        }
        catch (SecurityTokenMalformedException ex)
        {
            throw new UnauthorizedException(LoggingEvents.Account.InvalidClientToken);
        }

        if (string.IsNullOrEmpty(userId))
        {
            throw new UnauthorizedException(LoggingEvents.Account.InvalidClientToken);
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            throw new UnauthorizedException("未找到使用者");

        if (user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.Now)
            throw new UnauthorizedException(LoggingEvents.Account.InvalidClientToken);

        var tokenResponse = await GenerateTokenResponseAsync(user);

        return tokenResponse;
    }

    public async Task<bool> ConfirmEmailAsync(string email, string token)
    {
        var user = await _userManager.FindByEmailAsync(email);

        // 這段邏輯用於防止駭客根據錯誤訊息獲知帳號是否存在
        if (user == null)
        {
            // 模擬一個錯誤的檢查，讓駭客無法輕易分辨
            await Task.Delay(100);

            _logger.LogWarning(LoggingEvents.Account.UserNotFoundFormat, email);

            throw new UnauthorizedException(LoggingEvents.Account.InvalidClientToken);
        }

        try
        {
            token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
        }
        catch (FormatException)
        {
            throw new UnauthorizedException(LoggingEvents.Account.InvalidClientToken);
        }

        IdentityResult result = await _userManager.ConfirmEmailAsync(user, token);

        return result.Succeeded;
    }

    public async Task<bool> ResetPasswordAsync(string email, string resetCode, string newPassword)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user == null || !await _userManager.IsEmailConfirmedAsync(user))
        {
            // 模擬一個錯誤的檢查，讓駭客無法輕易分辨
            await Task.Delay(100);

            _logger.LogWarning(LoggingEvents.Account.ResetPasswordUseNonExistentEmailFormat, user);

            return false;
        }

        IdentityResult result;
        try
        {
            var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(resetCode));
            code = _dataProtectionService.Unprotect(code);
            result = await _userManager.ResetPasswordAsync(user, code, newPassword);
        }
        catch (FormatException)
        {
            result = IdentityResult.Failed(_userManager.ErrorDescriber.InvalidToken());
        }

        if (!result.Succeeded)
        {
            _logger.LogError(result.ToString());
        }

        return result.Succeeded;
    }

    public async Task<string?> GetUserNameAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        return user?.UserName;
    }

    public async Task<string?> GetUserIdAsync(string userName)
    {
        var user = await _userManager.FindByEmailAsync(userName);

        return user?.Id;
    }

    public async Task<(Result Result, string UserId)> CreateUserAsync(string userName, string password)
    {
        var user = new ApplicationUser
        {
            UserName = userName,
            Email = userName,
        };

        var result = await _userManager.CreateAsync(user, password);

        return (result.ToApplicationResult(), user.Id);
    }

    public async Task<bool> IsInRoleAsync(string userId, string role)
    {
        var user = await _userManager.FindByIdAsync(userId);

        return user != null && await _userManager.IsInRoleAsync(user, role);
    }

    public async Task<bool> AuthorizeAsync(string userId, string policyName)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return false;
        }

        var principal = await _userClaimsPrincipalFactory.CreateAsync(user);

        var result = await _authorizationService.AuthorizeAsync(principal, policyName);

        return result.Succeeded;
    }

    public async Task<Result> DeleteUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        return user != null ? await DeleteUserAsync(user) : Result.Success();
    }

    public async Task<Result> DeleteUserAsync(ApplicationUser user)
    {
        var result = await _userManager.DeleteAsync(user);

        return result.ToApplicationResult();
    }

    public async Task<bool> SendConfirmationEmailAsync(string userId, string email)
    {
        var token = await GenerateEmailConfirmationTokenAsync(userId);
        var confirmationLink = $"{_appConfig.SiteUrl}{_identitySettings.ConfirmEmailTokenUrl}?token={token}&email={email}";

        // 信件內容
        var letterModel = new ConfirmationEmailLetterModel()
        {
            SystemName = _appConfig.SystemName,
            SiteUrl = _appConfig.SiteUrl,
            UserName = email,
            ConfirmationLink = confirmationLink
        };

        // 取得範本
        var html = await _mailService.GetMailContentAsync(letterModel, "ConfirmationEmailLetter");

        return await _mailService.SendAsync(new MailRequest
        {
            To = email,
            Subject = $"歡迎加入 {_appConfig.SystemName}！請驗證你的電子郵件地址",
            Body = html,
        });
    }

    public async Task<bool> SendForgotPasswordEmailAsync(string userId, string email, string resetCodeLink)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            _logger.LogWarning(LoggingEvents.Account.SendForgotPasswordLetterUseNonExistentUserFormat, user);

            return false;
        }

        var resetCode = await _userManager.GeneratePasswordResetTokenAsync(user);
        resetCode = _dataProtectionService.Protect(resetCode);
        resetCode = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(resetCode));
        resetCodeLink = $"{resetCodeLink}?resetCode={resetCode}&email={email}";

        // 信件內容
        var letterModel = new ForgotPasswordLetterModel()
        {
            SystemName = _appConfig.SystemName,
            SiteUrl = _appConfig.SiteUrl,

            UserName = user.UserName ?? "",
            ResetCodeLink = resetCodeLink
        };

        // 取得範本
        var html = await _mailService.GetMailContentAsync(letterModel, "ForgotPasswordLetter");

        return await _mailService.SendAsync(new MailRequest
        {
            To = user.Email ?? "",
            Subject = $"重設你的 {_appConfig.SystemName} 密碼",
            Body = html,
        });
    }

    public async Task<AccessTokenResponse> GenerateTokenResponseAsync(ApplicationUser user)
    {
        var token = _jwtTokenService.GenerateAccessToken(await GetClaimsAsync(user));
        var refreshToken = _jwtTokenService.GenerateRefreshToken(user.Id);

        if (!int.TryParse(_configuration["JwtOptions:ExpiresInMinutes"], out int expiresInMinutes))
        {
            expiresInMinutes = 60;
        }

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.Now.AddMinutes(expiresInMinutes);

        await _userManager.UpdateAsync(user);

        var tokenResponse = new AccessTokenResponse
        {
            AccessToken = token,
            ExpiresIn = expiresInMinutes * 60,
            RefreshToken = refreshToken
        };

        return tokenResponse;
    }

    public async Task SignOutAsync()
    {
        await _signInManager.SignOutAsync();
        _logger.LogInformation("使用者已成功登出");
    }

    public bool IsSignedIn(ClaimsPrincipal user)
    {
        return _signInManager.IsSignedIn(user);
    }

    #region Private

    private async Task<(SignInResult Result, ApplicationUser? User)> ValidateUserAsync(string userName, string password)
    {
        var user = await _userManager.FindByNameAsync(userName);

        if (user == null)
        {
            await SimulateInvalidLoginDelay(userName);

            return (SignInResult.Failed, null);
        }

        if (await _userManager.IsLockedOutAsync(user))
        {
            _logger.LogWarning(LoggingEvents.Account.AccountLockedFormat, userName);

            return (SignInResult.LockedOut, user);
        }

        if (!await _userManager.IsEmailConfirmedAsync(user))
        {
            _logger.LogWarning(LoggingEvents.Account.EmailNotConfirmedFormat, userName);

            return (SignInResult.NotAllowed, user);
        }

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, password);
        if (!isPasswordValid)
        {
            LogLockoutOrEmailIssue(userName, await _userManager.IsLockedOutAsync(user), !await _userManager.IsEmailConfirmedAsync(user));

            return (SignInResult.Failed, user);
        }

        return (SignInResult.Success, user);
    }

    private async Task SimulateInvalidLoginDelay(string userName)
    {
        await Task.Delay(100);
        _logger.LogWarning(LoggingEvents.Account.UserNotFoundFormat, userName);
    }

    private void LogLockoutOrEmailIssue(string userName, bool isLockedOut, bool isEmailNotConfirmed)
    {
        if (isLockedOut)
        {
            _logger.LogWarning(LoggingEvents.Account.AccountLockedFormat, userName);
        }

        if (isEmailNotConfirmed)
        {
            _logger.LogWarning(LoggingEvents.Account.EmailNotConfirmedFormat, userName);
        }
    }

    private async Task<IEnumerable<Claim>> GetClaimsAsync(ApplicationUser user)
    {
        var userClaims = await _userManager.GetClaimsAsync(user);
        var roles = await _userManager.GetRolesAsync(user);
        var roleClaims = new List<Claim>();
        var permissionClaims = new List<Claim>();
        foreach (var role in roles)
        {
            roleClaims.Add(new Claim(ClaimTypes.Role, role));
            var thisRole = await _roleManager.FindByNameAsync(role);
            var allPermissionsForThisRoles = await _roleManager.GetClaimsAsync(thisRole);
            permissionClaims.AddRange(allPermissionsForThisRoles);
        }

        var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id),
                new(ClaimTypes.Email, user.Email),
            }
        .Union(userClaims)
        .Union(roleClaims)
        .Union(permissionClaims);

        return claims;
    }

    #endregion
}
