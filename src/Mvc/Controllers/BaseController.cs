using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
}
