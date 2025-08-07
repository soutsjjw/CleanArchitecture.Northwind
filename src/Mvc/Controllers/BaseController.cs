using CleanArchitecture.Northwind.Domain.Enums;
using CleanArchitecture.Northwind.Infrastructure.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Mvc.Controllers;

[Authorize]
public class BaseController<T> : Controller
{
    private IMediator _mediator;
    protected IMediator Mediator => _mediator ??= HttpContext.RequestServices.GetService<IMediator>();

    protected readonly ILogger<T> _logger;

    public BaseController(ILogger<T> logger)
    {
        _logger = logger;
    }

    public void GenerateGenderOptions()
    {
        var items = Enum.GetValues(typeof(Gender))
            .Cast<Gender>()
            .Select(g => new SelectListItem
            {
                Value = ((int)g).ToString(),
                Text = g.GetDisplayName()
            });
        ViewBag.GenderOptions = items;
    }
}
