using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Common.Interfaces.Identity;
using CleanArchitecture.Northwind.Application.Features.Account.Commands.ForgotPassword;
using CleanArchitecture.Northwind.Application.Features.Account.Commands.ResetPassword;
using CleanArchitecture.Northwind.Application.Features.Account.Commands.UserLogin;
using CleanArchitecture.Northwind.Application.Features.Account.Commands.UserRegister;
using CleanArchitecture.Northwind.Application.Features.Totp.Commands.EnableTotp;
using CleanArchitecture.Northwind.Application.Features.Totp.Commands.GenerateTotp;
using CleanArchitecture.Northwind.Application.Features.Totp.Commands.VerifyTotp;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Mvc.Extensions;
using Mvc.ViewModels;
using Mvc.ViewModels.Account;

namespace Mvc.Controllers;

public class AccountController : BaseController<AccountController>
{
    private readonly IUser _user;
    private readonly IIdentityService _identityService;
    private readonly ICloudflareService _cloudflareService;
    private readonly IAppConfigurationSettings _appConfig;
    private readonly IIdentitySettings _identitySettings;

    private const string _AuthenticationFailedMessage = "驗證失敗，請重新嘗試";

    public AccountController(IUser user,
        IIdentityService identityService,
        ICloudflareService cloudflareService,
        IAppConfigurationSettings appConfig,
        IIdentitySettings identitySettings,
        ILogger<AccountController> logger)
        : base(logger)
    {
        _user = user;
        _identityService = identityService;
        _cloudflareService = cloudflareService;
        _appConfig = appConfig;
        _identitySettings = identitySettings;
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
    public async Task<IActionResult> LoginAsync([FromForm] LoginViewModel viewModel, [FromQuery] string returnUrl = null)
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

        var result = await Mediator.Send(new UserLoginCommand { UserName = viewModel.Email, Password = viewModel.Password });
        if (!result.Succeeded)
        {
            return View(viewModel)
                .WithError(result.Errors.ToList(), "登入失敗");
        }

        if (result.Data.User.Profile.IsTotpEnabled)
        {
            // 導向 TOTP 驗證頁
            TempData["UserId"] = result.Data.User.Id;
            TempData["ReturnUrl"] = returnUrl;
            return RedirectToAction("VerifyTotp");
        }
        else
        {
            if (_identitySettings.ForceEnableTotp)
            {
                // 導向 TOTP 設定頁
                TempData["UserId"] = result.Data.User.Id;
                TempData["ReturnUrl"] = returnUrl;
                return RedirectToAction("SetupTotp");
            }
        }

        return RedirectToAction("Index");
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

    /// <summary>
    /// 非同步驗證 Cloudflare Turnstile CAPTCHA 回應。
    /// </summary>
    /// <remarks>此方法檢查請求表單中的 CAPTCHA 回應令牌，並使用 Cloudflare 服務進行驗證。如果令牌缺失或無效，則會將身分驗證錯誤新增至模型狀態。 </remarks>
    /// 如果 CAPTCHA 回應有效，則傳回 <returns><see langword="true"/>；否則，傳回 <see langword="false"/>。 </returns>
    private async Task<bool> VerifyTurnstileAsync()
    {
        var token = Request.Form["cf-turnstile-response"];
        if (string.IsNullOrEmpty(token))
        {
            ModelState.AddModelError(string.Empty, _AuthenticationFailedMessage);
            return false;
        }

        var isValidCaptcha = await _cloudflareService.VerifyTurnstileAsync(token);

        if (!isValidCaptcha)
        {
            ModelState.AddModelError(string.Empty, _AuthenticationFailedMessage);
            return false;
        }

        return true;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> SetupTotp()
    {
        var userId = _user.Id ?? TempData["UserId"]?.ToString();
        if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login");

        var applicationUser = await _identityService.GetUserByIdAsync(userId);
        if (applicationUser == null)
        {
            return RedirectToAction("Login")
                .WithError(this, "使用者不存在或已被刪除");
        }

        if (applicationUser.Profile.IsTotpEnabled)
        {
            return RedirectToAction("VerifyTotp")
                .WithError(this, "TOTP 已經啟用，請先驗證 TOTP");
        }

        var result = await Mediator.Send(new GenerateTotpCommand { UserId = userId });

        if (!result.Succeeded)
        {
            return View().WithError(result.Errors.ToList(), "產生 TOTP 失敗");
        }

        var viewModel = new SetupTotpViewModel
        {
            QrCodeImage = result.Data.QrCodeImage,
            ManualEntryKey = result.Data.ManualEntryKey,
        };

        if (_user.Id == null)
            TempData["UserId"] = userId;

        return View(viewModel);
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetupTotp(SetupTotpViewModel viewModel)
    {
        var userId = _user.Id ?? TempData["UserId"]?.ToString();
        if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login");

        var result = await Mediator.Send(new EnableTotpCommand
        {
            UserId = userId,
            Code = viewModel.Code,
        });

        if (result.Succeeded)
        {
            return RedirectToAction("VerifyTotp")
                .WithSuccess(this, "啟用 TOTP 成功");
        }

        return View(viewModel).WithError(result.Errors.ToList(), "啟用 TOTP 失敗");
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult VerifyTotp()
    {
        var userId = TempData["UserId"]?.ToString();
        if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login");

        TempData["UserId"] = userId;

        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifyTotp(TotpVerifyViewModel viewModel)
    {
        var userId = TempData["UserId"]?.ToString();
        if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login");

        var result = await Mediator.Send(new VerifyTotpCommand
        {
            UserId = userId,
            Code = viewModel.Code
        });

        if (result.Succeeded)
        {
            return RedirectToActionPermanent("Index", "Home");
        }

        TempData["UserId"] = userId;

        ModelState.AddModelError(nameof(viewModel.Code), "");

        return View(viewModel).WithError("驗證碼錯誤，請重新輸入");
    }
}
