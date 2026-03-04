using BlazorWeatherApp.Models;

namespace BlazorWeatherApp.Services;

public interface IWeatherForecastService
{
    Task<WeatherForecast[]> GetForecastsAsync(DateOnly startDate);
}
