using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Features.Account.Commands.ForgotPassword;
using CleanArchitecture.Northwind.Application.Features.Account.Commands.ResetPassword;
using CleanArchitecture.Northwind.Application.Features.Account.Commands.UserLogin;
using CleanArchitecture.Northwind.Application.Features.Account.Commands.UserRegister;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Mvc.Extensions;
using Mvc.ViewModels;

namespace Mvc.Controllers;

public class AccountController : BaseController<AccountController>
{
    private readonly IIdentityService _identityService;
    private readonly ICloudflareService _cloudflareService;
    private readonly IAppConfigurationSettings _appConfig;

    private const string _AuthenticationFailedMessage = "驗證失敗，請重新嘗試";

    public AccountController(IIdentityService identityService,
        ICloudflareService cloudflareService,
        IAppConfigurationSettings appConfig, ILogger<AccountController> logger)
        : base(logger)
    {
        _identityService = identityService;
        _cloudflareService = cloudflareService;
        _appConfig = appConfig;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Index()
    {
        if (_identityService.IsSignedIn(User))
            return RedirectToAction("Index", "Home");

        return RedirectToAction("Login");
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> LoginAsync(string returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");

        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
        await _identityService.SignOutAsync();

        ViewData["ReturnUrl"] = returnUrl;

        return View(new LoginViewModel());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login([FromForm] LoginViewModel viewModel, [FromQuery] string returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");

        if (!await VerifyTurnstileAsync())
        {
            return View(viewModel).WithError(_AuthenticationFailedMessage);
        }

        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        System.Threading.Thread.Sleep(5000);

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
            //ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        //    return Page();
        //}

        return View(viewModel)
            .WithError(result.Errors.ToList(), "登入失敗");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LogoutAsync()
    {
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
        await _identityService.SignOutAsync();

        return RedirectToAction("Login").WithSuccess(this, "登出成功");
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            // 如果前端驗證失敗，返回 View 並顯示錯誤訊息
            return View(model);
        }

        // 將 ViewModel 轉換為 Command
        var command = new RegisterUserCommand
        {
            Email = model.Email,
            Password = model.Password,
            FullName = model.FullName,
            IDNo = model.IDNo,
            Title = model.Title
        };

        // 呼叫 Application 層的處理器
        var result = await Mediator.Send(command);

        if (!result.Succeeded)
        {
            // 如果註冊失敗，將錯誤訊息加入 ModelState 並返回 View
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }
            return View(model).WithError(ModelState.CollectErrorMessages());
        }

        // 註冊成功，重定向至其他頁面或顯示成功訊息
        return RedirectToAction("Login").WithSuccess(this, "註冊帳號成功");
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ForgotPassword()
    {
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel viewModel)
    {
        if (!await VerifyTurnstileAsync())
        {
            return View(viewModel).WithError(_AuthenticationFailedMessage);
        }

        if (!ModelState.IsValid)
        {
            return View(viewModel).WithError(ModelState.CollectErrorMessages());
        }

        var result = await Mediator.Send(new ForgotPasswordCommand
        {
            Email = viewModel.Email,
            Link = $"{_appConfig.SiteUrl}/Account/ResetPassword"
        });

        System.Threading.Thread.Sleep(5000);

        if (result.Succeeded)
        {
            // 寄送成功，導向提示頁面
            return RedirectToAction("ForgotPasswordConfirmation");
        }
        else
        {
            // 寄送失敗，顯示錯誤訊息
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }
            return View(viewModel).WithError(ModelState.CollectErrorMessages());
        }
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ForgotPasswordConfirmation()
    {
        return View();
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ResetPassword(string email, string resetCode)
    {
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            return View(viewModel).WithError(ModelState.CollectErrorMessages());
        }

        var result = await Mediator.Send(new ResetPasswordCommand
        {
            Email = viewModel.Email,
            ResetCode = viewModel.ResetCode,
            NewPassword = viewModel.NewPassword,
            ConfirmPassword = viewModel.ConfirmPassword
        });

        if (result.Succeeded)
        {
            // 寄送成功，導向提示頁面
            return RedirectToAction("Login").WithSuccess(this, "重設密碼成功");
        }
        else
        {
            // 寄送失敗，顯示錯誤訊息
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }
            return View(viewModel).WithError(ModelState.CollectErrorMessages());
        }
    }

    private async Task<bool> VerifyTurnstileAsync()
    {
        var token = Request.Form["cf-turnstile-response"];
        var isValidCaptcha = await _cloudflareService.VerifyTurnstileAsync(token);

        if (!isValidCaptcha)
        {
            ModelState.AddModelError(string.Empty, _AuthenticationFailedMessage);
            return false;
        }

        return true;
    }
}
