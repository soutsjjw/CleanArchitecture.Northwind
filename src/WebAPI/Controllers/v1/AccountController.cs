using System.ComponentModel.DataAnnotations;
using System.Text;
using CleanArchitecture.Northwind.Application.Account.Commands.ConfirmEmail;
using CleanArchitecture.Northwind.Application.Account.Commands.Refresh;
using CleanArchitecture.Northwind.Application.Account.Commands.ResendConfirmationEmail;
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
        var result = await Mediator.Send(new RefreshCommand { RefreshToken = refreshRequest.RefreshToken });

        return Ok(result);
    }

    [AllowAnonymous]
    [HttpGet("confirm-email")]
    public async Task<IActionResult> ConfirmEmail([FromQuery] string email, [FromQuery] string token, [FromQuery] string? changedEmail)
    {
        var result = await Mediator.Send(new ConfirmEmailCommand { Email = email, Token = token });

        return Ok(result);
    }

    [AllowAnonymous]
    [HttpGet("resend-confirmation-email")]
    public async Task<IActionResult> ResendConfirmationEmail([FromQuery] string email)
    {
        var result = await Mediator.Send(new ResendConfirmationEmailCommand { Email = email });

        return Ok(result);
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
}
