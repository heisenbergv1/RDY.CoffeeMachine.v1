using CoffeeMachine.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace CoffeeMachine.Infrastructure.External;

public class OpenWeatherMapClient : IWeatherClient
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public OpenWeatherMapClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        // Read API key from configuration section "Weather:ApiKey"
        _apiKey = configuration["Weather:ApiKey"] 
                  ?? throw new ArgumentNullException("Weather:ApiKey not configured in appsettings.json");
    }

    public async Task<double?> GetCurrentTemperatureAsync(string city)
    {
        try
        {
            var url = $"https://api.openweathermap.org/data/2.5/weather?q={city}&units=metric&appid={_apiKey}";
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) return null;

            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            return doc.RootElement.GetProperty("main").GetProperty("temp").GetDouble();
        }
        catch
        {
            return null;
        }
    }
}
