using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mvc.Controllers;

[AllowAnonymous]
public class HomeController : BaseController<HomeController>
{
    public HomeController(ILogger<HomeController> logger)
        : base(logger)
    {
    }

    public IActionResult Index()
    {
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
