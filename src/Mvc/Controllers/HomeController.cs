using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NToastNotify;

namespace Mvc.Controllers;

[AllowAnonymous]
public class HomeController : BaseController<HomeController>
{
    public HomeController(IToastNotification toastNotification, ILogger<HomeController> logger)
        : base(toastNotification, logger)
    {
    }

    public IActionResult Index()
    {
        ////Testing Default Methods

        ////Success
        //_toastNotification.AddSuccessToastMessage("Same for success message");
        //// Success with default options (taking into account the overwritten defaults when initializing in Startup.cs)
        //_toastNotification.AddSuccessToastMessage();

        ////Info
        //_toastNotification.AddInfoToastMessage();

        ////Warning
        //_toastNotification.AddWarningToastMessage();

        ////Error
        //_toastNotification.AddErrorToastMessage();

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult TermsOfService()
    {
        return View();
    }
}
