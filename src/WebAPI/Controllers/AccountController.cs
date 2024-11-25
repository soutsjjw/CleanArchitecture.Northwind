using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text;
using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Common.Models;
using CleanArchitecture.Northwind.Application.Common.Settings;
using CleanArchitecture.Northwind.Infrastructure.Identity;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CleanArchitecture.Northwind.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IUserStore<ApplicationUser> _userStore;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IMailService _mailService;
    private readonly IOptions<AppConfigurationSettings> _appConfig;

    private static readonly EmailAddressAttribute _emailAddressAttribute = new();

    public AccountController(
        IConfiguration configuration,
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IUserStore<ApplicationUser> userStore,
        IJwtTokenService jwtTokenService,
        IMailService mailService,
        IOptions<AppConfigurationSettings> appConfig)
    {
        _configuration = configuration;
        _signInManager = signInManager;
        _userManager = userManager;
        _roleManager = roleManager;
        _userStore = userStore;
        _jwtTokenService = jwtTokenService;
        _mailService = mailService;
        _appConfig = appConfig;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest registration)
    {
        if (!_userManager.SupportsUserEmail)
        {
            return BadRequest("User store with email support is required.");
        }

        var emailStore = (IUserEmailStore<ApplicationUser>)_userStore;
        var email = registration.Email;

        if (string.IsNullOrEmpty(email) || !_emailAddressAttribute.IsValid(email))
        {
            return ValidationProblem(IdentityResult.Failed(_userManager.ErrorDescriber.InvalidEmail(email)).ToString());
        }

        var user = new ApplicationUser();
        await _userStore.SetUserNameAsync(user, email, CancellationToken.None);
        await emailStore.SetEmailAsync(user, email, CancellationToken.None);
        var result = await _userManager.CreateAsync(user, registration.Password);

        if (!result.Succeeded)
        {
            return ValidationProblem(result.ToString());
        }

        await SendConfirmationEmailAsync(user);

        return Ok();
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult> Login([FromBody] LoginRequest login, [FromQuery] bool? useCookies, [FromQuery] bool? useSessionCookies)
    {
        var user = await _userManager.FindByEmailAsync(login.Email);

        if (user == null)
        {
            return Unauthorized("Invalid login attempt.");
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, login.Password, lockoutOnFailure: true);

        if (result.RequiresTwoFactor)
        {
            if (!string.IsNullOrEmpty(login.TwoFactorCode))
            {
                result = await _signInManager.TwoFactorAuthenticatorSignInAsync(login.TwoFactorCode, false, rememberClient: false);
            }
            else if (!string.IsNullOrEmpty(login.TwoFactorRecoveryCode))
            {
                result = await _signInManager.TwoFactorRecoveryCodeSignInAsync(login.TwoFactorRecoveryCode);
            }
        }

        if (!result.Succeeded)
        {
            return Problem(result.ToString(), statusCode: StatusCodes.Status401Unauthorized);
        }

        var tokenResponse = await GenerateTokenResponseAsync(user);

        return Ok(tokenResponse);
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest refreshRequest)
    {
        if (refreshRequest is null)
        {
            return Unauthorized("Invalid Client Token.");
        }

        ClaimsPrincipal userPrincipal = null;
        string userId = string.Empty;

        try
        {
            userPrincipal = _jwtTokenService.GetPrincipalFromExpiredToken(refreshRequest.RefreshToken);
            userId = userPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
        }
        catch (SecurityTokenSignatureKeyNotFoundException ex)
        {
            return Unauthorized("Invalid Client Token.");
        }
        catch (SecurityTokenMalformedException ex)
        {
            return Unauthorized("Invalid Client Token.");
        }

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("Invalid Client Token.");
        }

        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
            return Unauthorized("User Not Found.");

        if (user.RefreshToken != refreshRequest.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.Now)
            return Unauthorized("Invalid Client Token.");

        var tokenResponse = await GenerateTokenResponseAsync(user);

        return Ok(tokenResponse);
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

    private async Task SendConfirmationEmailAsync(ApplicationUser user, bool resend = false)
    {
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = System.Net.WebUtility.UrlEncode(token);
        var confirmEmailUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/api/account/confirm-email;token={encodedToken}";

        var subject = $"歡迎加入 {_appConfig.Value.ApplicationName}！請驗證你的電子郵件地址";
        if (resend) subject += " - Resend";

        var sbBody = new StringBuilder();
        sbBody.AppendLine($"親愛的 {user.UserName}：");
        sbBody.AppendLine("<br /><br />");
        sbBody.AppendLine($"感謝你註冊 {_appConfig.Value.ApplicationName}！我們非常高興你加入我們的社群。");
        sbBody.AppendLine("為了完成註冊，我們需要你驗證你的電子郵件地址。請點擊下方的鏈接來驗證你的賬戶：");
        sbBody.AppendLine("<br /><br />");
        sbBody.AppendLine($"<a href='{confirmEmailUrl}'>驗證你的電子郵件地址</a>");
        sbBody.AppendLine("<br /><br />");
        sbBody.AppendLine("如果你無法點擊上面的鏈接，請將以下網址複製並粘貼到你的瀏覽器中：");
        sbBody.AppendLine("<br /><br />");
        sbBody.AppendLine($"{confirmEmailUrl}");
        sbBody.AppendLine("<br /><br />");
        sbBody.AppendLine($"如果你並未註冊 {_appConfig.Value.ApplicationName}，請忽略此郵件。我們對任何不便表示歉意。");
        sbBody.AppendLine("<br /><br />");
        sbBody.AppendLine("感謝你的配合！");
        sbBody.AppendLine("<br /><br />");
        sbBody.AppendLine($"最好的祝福，{_appConfig.Value.ApplicationName} 團隊");

        await _mailService.SendAsync(new MailRequest
        {
            To = user.Email,
            Subject = subject,
            Body = sbBody.ToString(),
        });
    }

    #endregion
}
