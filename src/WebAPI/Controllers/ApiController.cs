using Microsoft.AspNetCore.Mvc;

namespace CleanArchitecture.Northwind.WebAPI.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class ApiController : ControllerBase
{
    private IMediator _mediator;
    protected IMediator Mediator => _mediator ??= HttpContext.RequestServices.GetService<IMediator>();

    /// <summary>
    /// 建構式
    /// </summary>
    public ApiController()
    {
    }
}
