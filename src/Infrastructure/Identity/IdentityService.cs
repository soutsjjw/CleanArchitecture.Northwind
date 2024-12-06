using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text;
using CleanArchitecture.Northwind.Application.Common.Exceptions;
using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Common.Logging;
using CleanArchitecture.Northwind.Application.Common.Models;
using CleanArchitecture.Northwind.Application.Common.Models.Letter;
using CleanArchitecture.Northwind.Application.Common.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CleanArchitecture.Northwind.Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly IConfiguration _configuration;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IUserStore<ApplicationUser> _userStore;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IUserClaimsPrincipalFactory<ApplicationUser> _userClaimsPrincipalFactory;
    private readonly IAuthorizationService _authorizationService;
    private readonly IMailService _mailService;

    private static readonly EmailAddressAttribute _emailAddressAttribute = new();

    private readonly ILogger<IdentityService> _logger;
    private readonly IOptions<AppConfigurationSettings> _appConfig;

    public IdentityService(
        IConfiguration configuration,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        RoleManager<IdentityRole> roleManager,
        IUserStore<ApplicationUser> userStore,
        IJwtTokenService jwtTokenService,
        IUserClaimsPrincipalFactory<ApplicationUser> userClaimsPrincipalFactory,
        IAuthorizationService authorizationService,
        IMailService mailService,
        ILogger<IdentityService> logger,
        IOptions<AppConfigurationSettings> appConfig)
    {
        _configuration = configuration;
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _userStore = userStore;

        _jwtTokenService = jwtTokenService;

        _userClaimsPrincipalFactory = userClaimsPrincipalFactory;
        _authorizationService = authorizationService;
        _mailService = mailService;

        _logger = logger;
        _appConfig = appConfig;
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

    public async Task<AccessTokenResponse> UserLoginByAPI(string userName, string password)
    {
        var user = await _userManager.FindByNameAsync(userName);

        // 這段邏輯用於防止駭客根據錯誤訊息獲知帳號是否存在
        if (user == null)
        {
            // 模擬一個錯誤的檢查，讓駭客無法輕易分辨
            await Task.Delay(100);

            _logger.LogWarning(LoggingEvents.Account.UserNotFoundFormat, userName);

            throw new UnauthorizedException(LoggingEvents.Account.InvalidLoginAttempt);
        }

        // 密碼正確但帳號被鎖定或Email未認證的情況
        var isLockedOutAsync = await _userManager.IsLockedOutAsync(user);
        var isEmailNotConfirmed = !await _userManager.IsEmailConfirmedAsync(user);

        if (isLockedOutAsync || isEmailNotConfirmed)
        {
            var isPasswordCorrect = await _userManager.CheckPasswordAsync(user, password);

            // 僅當密碼正確時才拋出具體的錯誤
            if (isPasswordCorrect)
            {
                if (isLockedOutAsync)
                {
                    _logger.LogWarning(LoggingEvents.Account.AccountLockedFormat, userName);
                    throw new UnauthorizedException(LoggingEvents.Account.AccountLocked);
                }

                if (isEmailNotConfirmed)
                {
                    _logger.LogWarning(LoggingEvents.Account.EmailNotConfirmedFormat, userName);
                    throw new UnauthorizedException(LoggingEvents.Account.EmailNotConfirmed);
                }
            }
        }

        var result = await _signInManager.PasswordSignInAsync(user, password, isPersistent: false, lockoutOnFailure: true);

        // 密碼錯誤或其他登入失敗的情況
        if (!result.Succeeded)
        {
            _logger.LogWarning(LoggingEvents.Account.InvalidLoginAttemptFormat, userName);
            throw new UnauthorizedException(LoggingEvents.Account.InvalidLoginAttempt);
        }

        // 生成 Token
        var tokenResponse = await GenerateTokenResponseAsync(user);
        return tokenResponse;
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

    public async Task<bool> ConfirmEmail(string email, string token)
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

    public async Task<bool> ResendConfirmationEmail(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        var username = email;

        // 這段邏輯用於防止駭客根據錯誤訊息獲知帳號是否存在
        if (user == null)
        {
            // 模擬一個錯誤的檢查，讓駭客無法輕易分辨
            await Task.Delay(100);

            _logger.LogWarning(LoggingEvents.Account.SendConfirmLetterUseNonExistentEmailFormat, username);

            return false;
        }

        return await SendConfirmationEmailAsync(user.Id, email);
    }

    public async Task<string?> GetUserNameAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        return user?.UserName;
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
        var confirmationLink = $"{_appConfig.Value.SiteUrl}/api/Account/ConfirmEmail?token={token}&email={email}";

        // 信件內容
        var letterModel = new ConfirmationEmailLetterModel()
        {
            SystemName = _appConfig.Value.SystemName,
            SiteUrl = _appConfig.Value.SiteUrl,
            UserName = email,
            ConfirmationLink = confirmationLink
        };

        // 取得範本
        var html = await _mailService.GetMailContentAsync(letterModel, "ConfirmationEmailLetter");

        return await _mailService.SendAsync(new MailRequest
        {
            To = email,
            Subject = $"歡迎加入 {_appConfig.Value.SystemName}！請驗證你的電子郵件地址",
            Body = html,
        });
    }

    #region Private

    private async Task<AccessTokenResponse> GenerateTokenResponseAsync(ApplicationUser user)
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
