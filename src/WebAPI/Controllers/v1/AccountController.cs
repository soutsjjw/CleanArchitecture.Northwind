using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text;
using CleanArchitecture.Northwind.Application.Account.Commands.UserLogin;
using CleanArchitecture.Northwind.Application.Account.Commands.UserRegister;
using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Common.Models;
using CleanArchitecture.Northwind.Application.Common.Models.Letter;
using CleanArchitecture.Northwind.Application.Common.Settings;
using CleanArchitecture.Northwind.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CleanArchitecture.Northwind.WebAPI.Controllers.v1;

[ApiVersion("1.0")]
public class AccountController : ApiController
{
    private readonly IConfiguration _configuration;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IUserStore<ApplicationUser> _userStore;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IMailService _mailService;
    private readonly IFileService _fileService;
    private readonly IOptions<AppConfigurationSettings> _appConfig;
    private readonly IIdentityService _identityService;

    private static readonly EmailAddressAttribute _emailAddressAttribute = new();

    public AccountController(
        IConfiguration configuration,
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IUserStore<ApplicationUser> userStore,
        IJwtTokenService jwtTokenService,
        IMailService mailService,
        IFileService fileService,
        IIdentityService identityService,
        IOptions<AppConfigurationSettings> appConfig)
    {
        _configuration = configuration;
        _signInManager = signInManager;
        _userManager = userManager;
        _roleManager = roleManager;
        _userStore = userStore;
        _jwtTokenService = jwtTokenService;
        _mailService = mailService;
        _fileService = fileService;
        _identityService = identityService;

        _appConfig = appConfig;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserRegisterCommand request)
    {
        var result = await Mediator.Send(request);

        return Ok(result);
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult> Login([FromBody] LoginRequest login, [FromQuery] bool? useCookies, [FromQuery] bool? useSessionCookies)
    {
        var result = await Mediator.Send(new UserLoginCommand { UserName = login.Email, Password = login.Password });

        return Ok(result);
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

    [AllowAnonymous]
    [HttpGet("confirm-email")]
    public async Task<IActionResult> ConfirmEmail([FromQuery] string email, [FromQuery] string token, [FromQuery] string? changedEmail)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            // We could respond with a 404 instead of a 401 like Identity UI, but that feels like unnecessary information.
            return Unauthorized();
        }

        try
        {
            token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
        }
        catch (FormatException)
        {
            return Unauthorized();
        }

        IdentityResult result;

        if (string.IsNullOrEmpty(changedEmail))
        {
            result = await _userManager.ConfirmEmailAsync(user, token);
        }
        else
        {
            // As with Identity UI, email and user name are one and the same. So when we update the email,
            // we need to update the user name.
            result = await _userManager.ChangeEmailAsync(user, changedEmail, token);

            if (result.Succeeded)
            {
                result = await _userManager.SetUserNameAsync(user, changedEmail);
            }
        }

        if (!result.Succeeded)
        {
            return Unauthorized();
        }

        return Content("Thank you for confirming your email.");
    }

    [AllowAnonymous]
    [HttpGet("resend-confirmation-email")]
    public async Task<IActionResult> ResendConfirmationEmail([FromQuery] string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return Unauthorized();
        }

        await SendConfirmationEmailAsync(user, true);

        return Ok();
    }

    [AllowAnonymous]
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest resetRequest)
    {
        var user = await _userManager.FindByEmailAsync(resetRequest.Email);

        if (user != null && await _userManager.IsEmailConfirmedAsync(user))
        {
            var resetCode = await _userManager.GeneratePasswordResetTokenAsync(user);
            resetCode = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(resetCode));

            var letterModel = new ForgotPasswordLetterModel()
            {
                SystemName = _appConfig.Value.SystemName,
                SiteUrl = _appConfig.Value.SiteUrl,

                UserName = user.UserName ?? "",
                ResetCodeLink = Url.Action("ResetPassword", "Account", new { resetCode, email = user.Email }, Request.Scheme) ?? ""
            };

            await _mailService.SendAsync(new MailRequest
            {
                To = user.Email ?? "",
                Subject = $"重設你的 {_appConfig.Value.SystemName} 密碼",
                Body = await _mailService.GetMailContentAsync(letterModel, "ForgotPasswordLetter"),
            });
        }

        // Don't reveal that the user does not exist or is not confirmed
        return Ok();
    }

    [AllowAnonymous]
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest resetRequest)
    {
        var user = await _userManager.FindByEmailAsync(resetRequest.Email);

        if (user == null || !await _userManager.IsEmailConfirmedAsync(user))
        {
            // Don't reveal that the user does not exist or is not confirmed
            return ValidationProblem(IdentityResult.Failed(_userManager.ErrorDescriber.InvalidToken()).ToString());
        }

        IdentityResult result;
        try
        {
            var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(resetRequest.ResetCode));
            result = await _userManager.ResetPasswordAsync(user, code, resetRequest.NewPassword);
        }
        catch (FormatException)
        {
            result = IdentityResult.Failed(_userManager.ErrorDescriber.InvalidToken());
        }

        if (!result.Succeeded)
        {
            return ValidationProblem(result.ToString());
        }

        return Ok();
    }

    #region Private

    private async Task<Microsoft.AspNetCore.Authentication.BearerToken.AccessTokenResponse> GenerateTokenResponseAsync(ApplicationUser user)
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

        var tokenResponse = new Microsoft.AspNetCore.Authentication.BearerToken.AccessTokenResponse
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
        token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

        // 信件內容
        var letterModel = new ConfirmationEmailLetterModel()
        {
            SystemName = _appConfig.Value.SystemName,
            SiteUrl = _appConfig.Value.SiteUrl,

            UserName = user.UserName ?? "",
            ConfirmationLink = Url.Action("ConfirmEmail", "Account", new { token, email = user.Email }, Request.Scheme) ?? ""
        };

        // 取得範本
        var html = await _mailService.GetMailContentAsync(letterModel, "ConfirmationEmailLetter");

        await _mailService.SendAsync(new MailRequest
        {
            To = user.Email ?? "",
            Subject = $"歡迎加入 {_appConfig.Value.SystemName}！請驗證你的電子郵件地址",
            Body = html,
        });
    }

    #endregion
}
