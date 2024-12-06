using CleanArchitecture.Northwind.Application.Features.Account.Commands.ConfirmEmail;
using CleanArchitecture.Northwind.Application.Features.Account.Commands.ForgotPassword;
using CleanArchitecture.Northwind.Application.Features.Account.Commands.Refresh;
using CleanArchitecture.Northwind.Application.Features.Account.Commands.ResendConfirmationEmail;
using CleanArchitecture.Northwind.Application.Features.Account.Commands.ResetPassword;
using CleanArchitecture.Northwind.Application.Features.Account.Commands.UserLogin;
using CleanArchitecture.Northwind.Application.Features.Account.Commands.UserRegister;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitecture.Northwind.WebAPI.Controllers.v1;

[ApiVersion("1.0")]
public class AccountController : ApiController
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        IConfiguration configuration,
        ILogger<AccountController> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserRegisterCommand request)
    {
        request.ConfirmationLink = Url.Action("ConfirmEmail", "Account", null, Request.Scheme) ?? "";

        return Ok(await Mediator.Send(request));
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
        var confirmationLink = Url.Action("ConfirmEmail", "Account", null, Request.Scheme) ?? "";
        var result = await Mediator.Send(new ResendConfirmationEmailCommand { Email = email, ConfirmationLink = confirmationLink });

        return Ok(result);
    }

    [AllowAnonymous]
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest resetRequest)
    {
        var resetCodeLink = Url.Action("ResetPassword", "Account", null, Request.Scheme) ?? "";
        var result = await Mediator.Send(new ForgotPasswordCommand { Email = resetRequest.Email, ResetCodeLink = resetCodeLink });

        return Ok(result);
    }

    [AllowAnonymous]
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest resetRequest)
    {
        var result = await Mediator.Send(new ResetPasswordCommand { Email = resetRequest.Email, ResetCode = resetRequest.ResetCode, NewPassword = resetRequest.NewPassword });

        return Ok(result);
    }
}
