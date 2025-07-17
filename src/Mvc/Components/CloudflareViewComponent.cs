using CleanArchitecture.Northwind.Infrastructure.Configurations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Mvc.Components;

public class CloudflareViewComponent : ViewComponent
{
    private readonly CloudflareOptions _options;

    public CloudflareViewComponent(IOptions<CloudflareOptions> options)
    {
        _options = options.Value;

        Console.WriteLine($"[CloudflareViewComponent] SiteKey: {_options.SiteKey}");
    }

    public IViewComponentResult Invoke()
    {
        return View("Default", _options.SiteKey);
    }
}
