namespace CoffeeMachine.Application.Interfaces;

/// <summary>
/// Weather service client to fetch current temperature.
/// </summary>
public interface IWeatherClient
{
    Task<double?> GetCurrentTemperatureAsync(string city);
}