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
        // Arrange
        var startDate = DateOnly.FromDateTime(DateTime.Now);

        // Act
        var result = await _service.GetForecastsAsync(startDate);

        // Assert
        Assert.Equal(5, result.Length);
    }

    [Fact]
    public async Task GetForecastsAsync_ReturnsNonEmptyList()
    {
        // Arrange
        var startDate = DateOnly.FromDateTime(DateTime.Now);

        // Act
        var result = await _service.GetForecastsAsync(startDate);

        // Assert
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task GetForecastsAsync_DatesAreInFuture()
    {
        // Arrange
        var startDate = DateOnly.FromDateTime(DateTime.Now);

        // Act
        var result = await _service.GetForecastsAsync(startDate);

        // Assert
        foreach (var forecast in result)
        {
            Assert.True(forecast.Date > startDate,
                $"Expected date {forecast.Date} to be after {startDate}");
        }
    }

    [Fact]
    public async Task GetForecastsAsync_TemperaturesAreInValidRange()
    {
        // Arrange
        var startDate = DateOnly.FromDateTime(DateTime.Now);

        // Act
        var result = await _service.GetForecastsAsync(startDate);

        // Assert
        foreach (var forecast in result)
        {
            Assert.InRange(forecast.TemperatureC, -20, 54);
        }
    }

    [Fact]
    public async Task GetForecastsAsync_SummariesAreNotNull()
    {
        // Arrange
        var startDate = DateOnly.FromDateTime(DateTime.Now);

        // Act
        var result = await _service.GetForecastsAsync(startDate);

        // Assert
        foreach (var forecast in result)
        {
            Assert.NotNull(forecast.Summary);
            Assert.NotEmpty(forecast.Summary);
        }
    }

    [Fact]
    public async Task GetForecastsAsync_SummariesAreFromValidSet()
    {
        // Arrange
        var validSummaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild",
            "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };
        var startDate = DateOnly.FromDateTime(DateTime.Now);

        // Act
        var result = await _service.GetForecastsAsync(startDate);

        // Assert
        foreach (var forecast in result)
        {
            Assert.Contains(forecast.Summary, validSummaries);
        }
    }

    [Fact]
    public async Task GetForecastsAsync_DatesAreSequential()
    {
        // Arrange
        var startDate = DateOnly.FromDateTime(DateTime.Now);

        // Act
        var result = await _service.GetForecastsAsync(startDate);

        // Assert
        for (int i = 0; i < result.Length; i++)
        {
            Assert.Equal(startDate.AddDays(i + 1), result[i].Date);
        }
    }
}
