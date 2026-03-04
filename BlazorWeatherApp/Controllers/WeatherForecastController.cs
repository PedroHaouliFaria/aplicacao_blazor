using BlazorWeatherApp.Models;
using BlazorWeatherApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace BlazorWeatherApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WeatherForecastController : ControllerBase
{
    private readonly IWeatherForecastService _weatherService;

    public WeatherForecastController(IWeatherForecastService weatherService)
    {
        _weatherService = weatherService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<WeatherForecast>>> Get()
    {
        var startDate = DateOnly.FromDateTime(DateTime.Now);
        var forecasts = await _weatherService.GetForecastsAsync(startDate);
        return Ok(forecasts);
    }
}
