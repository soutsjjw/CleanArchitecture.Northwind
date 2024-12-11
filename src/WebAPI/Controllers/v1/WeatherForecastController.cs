using CleanArchitecture.Northwind.Application.Features.WeatherForecasts.Queries.GetWeatherForecasts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitecture.Northwind.WebAPI.Controllers.v1;

[ApiVersion("1.0")]
public class WeatherForecastController : ApiController
{
    private readonly ILogger<WeatherForecastController> _logger;

    public WeatherForecastController(ILogger<WeatherForecastController> logger)
    {
        _logger = logger;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    [Authorize]
    public async Task<IEnumerable<WeatherForecast>> Get(ISender sender)
    {
        _logger.LogInformation("使用 v1");

        return await sender.Send(new GetWeatherForecastsQuery());
    }
}
