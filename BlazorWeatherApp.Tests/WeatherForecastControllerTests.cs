using BlazorWeatherApp.Controllers;
using BlazorWeatherApp.Models;
using BlazorWeatherApp.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BlazorWeatherApp.Tests;

public class WeatherForecastControllerTests
{
    private readonly Mock<IWeatherForecastService> _mockService;
    private readonly WeatherForecastController _controller;

    public WeatherForecastControllerTests()
    {
        _mockService = new Mock<IWeatherForecastService>();
        _controller = new WeatherForecastController(_mockService.Object);
    }

    [Fact]
    public async Task Get_ReturnsOkResult_WithForecasts()
    {
        var expectedForecasts = new[]
        {
            new WeatherForecast { Date = DateOnly.FromDateTime(DateTime.Now.AddDays(1)), TemperatureC = 25, Summary = "Warm" },
            new WeatherForecast { Date = DateOnly.FromDateTime(DateTime.Now.AddDays(2)), TemperatureC = 30, Summary = "Hot" }
        };
        _mockService.Setup(s => s.GetForecastsAsync(It.IsAny<DateOnly>())).ReturnsAsync(expectedForecasts);

        var result = await _controller.Get();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var forecasts = Assert.IsAssignableFrom<IEnumerable<WeatherForecast>>(okResult.Value);
        Assert.Equal(2, forecasts.Count());
    }

    [Fact]
    public async Task Get_ReturnsNonEmptyList()
    {
        var expectedForecasts = new[]
        {
            new WeatherForecast { Date = DateOnly.FromDateTime(DateTime.Now.AddDays(1)), TemperatureC = 10, Summary = "Cool" }
        };
        _mockService.Setup(s => s.GetForecastsAsync(It.IsAny<DateOnly>())).ReturnsAsync(expectedForecasts);

        var result = await _controller.Get();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var forecasts = Assert.IsAssignableFrom<IEnumerable<WeatherForecast>>(okResult.Value);
        Assert.NotEmpty(forecasts);
    }

    [Fact]
    public async Task Get_CallsServiceExactlyOnce()
    {
        _mockService.Setup(s => s.GetForecastsAsync(It.IsAny<DateOnly>())).ReturnsAsync(Array.Empty<WeatherForecast>());

        await _controller.Get();

        _mockService.Verify(s => s.GetForecastsAsync(It.IsAny<DateOnly>()), Times.Once);
    }

    [Fact]
    public async Task Get_ReturnsCorrectForecastData()
    {
        var tomorrow = DateOnly.FromDateTime(DateTime.Now.AddDays(1));
        var expectedForecasts = new[]
        {
            new WeatherForecast { Date = tomorrow, TemperatureC = 20, Summary = "Mild" }
        };
        _mockService.Setup(s => s.GetForecastsAsync(It.IsAny<DateOnly>())).ReturnsAsync(expectedForecasts);

        var result = await _controller.Get();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var forecasts = Assert.IsAssignableFrom<IEnumerable<WeatherForecast>>(okResult.Value);
        var forecast = forecasts.First();
        Assert.Equal(tomorrow, forecast.Date);
        Assert.Equal(20, forecast.TemperatureC);
        Assert.Equal("Mild", forecast.Summary);
    }

    [Fact]
    public async Task Get_TemperatureF_IsCalculatedCorrectly()
    {
        var expectedForecasts = new[]
        {
            new WeatherForecast { Date = DateOnly.FromDateTime(DateTime.Now.AddDays(1)), TemperatureC = 0, Summary = "Freezing" },
            new WeatherForecast { Date = DateOnly.FromDateTime(DateTime.Now.AddDays(2)), TemperatureC = 100, Summary = "Scorching" }
        };
        _mockService.Setup(s => s.GetForecastsAsync(It.IsAny<DateOnly>())).ReturnsAsync(expectedForecasts);

        var result = await _controller.Get();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var forecasts = Assert.IsAssignableFrom<IEnumerable<WeatherForecast>>(okResult.Value).ToArray();
        Assert.Equal(32, forecasts[0].TemperatureF);
        Assert.Equal(32 + (int)(100 / 0.5556), forecasts[1].TemperatureF);
    }
}
