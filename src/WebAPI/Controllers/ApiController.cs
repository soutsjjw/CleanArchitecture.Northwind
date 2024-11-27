using Microsoft.AspNetCore.Mvc;

namespace CleanArchitecture.Northwind.WebAPI.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class ApiController : ControllerBase
{
    /// <summary>
    /// 建構式
    /// </summary>
    public ApiController()
    {
    }
}
