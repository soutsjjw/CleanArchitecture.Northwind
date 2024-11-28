using CleanArchitecture.Northwind.Application.WeatherForecasts.Queries.GetWeatherForecasts;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitecture.Northwind.WebAPI.Controllers.v2;

[ApiVersion("2.0")]
public class WeatherForecastController : ApiController
{
    private readonly ILogger<WeatherForecastController> _logger;

    public WeatherForecastController(ILogger<WeatherForecastController> logger)
    {
        _logger = logger;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public async Task<IEnumerable<WeatherForecast>> Get(ISender sender)
    {
        _logger.LogInformation("使用 v2");

        return await sender.Send(new GetWeatherForecastsQuery());
    }
}
