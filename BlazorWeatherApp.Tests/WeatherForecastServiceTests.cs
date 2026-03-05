using BlazorWeatherApp.Models;
using BlazorWeatherApp.Services;

namespace BlazorWeatherApp.Tests;

public class WeatherForecastServiceTests
{
    private readonly WeatherForecastService _service;

    public WeatherForecastServiceTests()
    {
        _service = new WeatherForecastService();
    }

    [Fact]
    public async Task GetForecastsAsync_ReturnsExactlyFiveForecasts()
    {
        var startDate = DateOnly.FromDateTime(DateTime.Now);
        var result = await _service.GetForecastsAsync(startDate);
        Assert.Equal(5, result.Length);
    }

    [Fact]
    public async Task GetForecastsAsync_ReturnsNonEmptyList()
    {
        var startDate = DateOnly.FromDateTime(DateTime.Now);
        var result = await _service.GetForecastsAsync(startDate);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task GetForecastsAsync_DatesAreInFuture()
    {
        var startDate = DateOnly.FromDateTime(DateTime.Now);
        var result = await _service.GetForecastsAsync(startDate);
        foreach (var forecast in result)
            Assert.True(forecast.Date > startDate);
    }

    [Fact]
    public async Task GetForecastsAsync_TemperaturesAreInValidRange()
    {
        var startDate = DateOnly.FromDateTime(DateTime.Now);
        var result = await _service.GetForecastsAsync(startDate);
        foreach (var forecast in result)
            Assert.InRange(forecast.TemperatureC, -20, 54);
    }

    [Fact]
    public async Task GetForecastsAsync_SummariesAreNotNull()
    {
        var startDate = DateOnly.FromDateTime(DateTime.Now);
        var result = await _service.GetForecastsAsync(startDate);
        foreach (var forecast in result)
        {
            Assert.NotNull(forecast.Summary);
            Assert.NotEmpty(forecast.Summary);
        }
    }

    [Fact]
    public async Task GetForecastsAsync_SummariesAreFromValidSet()
    {
        var validSummaries = new[] { "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching" };
        var startDate = DateOnly.FromDateTime(DateTime.Now);
        var result = await _service.GetForecastsAsync(startDate);
        foreach (var forecast in result)
            Assert.Contains(forecast.Summary, validSummaries);
    }

    [Fact]
    public async Task GetForecastsAsync_DatesAreSequential()
    {
        var startDate = DateOnly.FromDateTime(DateTime.Now);
        var result = await _service.GetForecastsAsync(startDate);
        for (int i = 0; i < result.Length; i++)
            Assert.Equal(startDate.AddDays(i + 1), result[i].Date);
    }
}
