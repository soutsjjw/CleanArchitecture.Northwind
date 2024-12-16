using CleanArchitecture.Northwind.Application.Features.Account.Commands.UserLogin;
using CleanArchitecture.Northwind.Domain.Entities.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Mvc.ViewModels;
using NToastNotify;

namespace Mvc.Controllers;

[AllowAnonymous]
public class AccountController : BaseController<AccountController>
{
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AccountController(SignInManager<ApplicationUser> signInManager, IToastNotification toastNotification, ILogger<AccountController> logger)
        : base(toastNotification, logger)
    {
        _signInManager = signInManager;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> LoginAsync(string returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");

        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

        ViewData["ReturnUrl"] = returnUrl;

        return View(new LoginViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> Login([FromForm] LoginViewModel viewModel, [FromQuery] string returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");

        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        var result = await Mediator.Send(new UserLoginCommand { UserName = viewModel.Email, Password = viewModel.Password });
        if (result.Succeeded)
        {
            _logger.LogInformation("User logged in.");
            return LocalRedirect(returnUrl);
        }
        //if (result.RequiresTwoFactor)
        //{
        //    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
        //}
        //if (result.IsLockedOut)
        //{
        //    _logger.LogWarning("User account locked out.");
        //    return RedirectToPage("./Lockout");
        //}
        //else
        //{
        //    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        //    return Page();
        //}

        base.ShowErrorToast("登入失敗", result.Errors?.ToList());

        return View(viewModel);
    }

    [HttpGet]
    public IActionResult Logout()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> LogoutAsync(string returnUrl)
    {
        await _signInManager.SignOutAsync();
        _logger.LogInformation("User logged out.");
        if (returnUrl != null)
        {
            return LocalRedirect(returnUrl);
        }
        else
        {
            return RedirectToAction("Login");
        }
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }
}
