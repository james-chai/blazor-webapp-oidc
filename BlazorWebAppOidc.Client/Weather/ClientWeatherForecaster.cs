using System.Net.Http.Json;

namespace BlazorWebAppOidc.Client.Weather;

internal sealed class ClientWeatherForecaster(HttpClient httpClient) : IWeatherForecaster
{
    public async Task<IEnumerable<WeatherForecast>> GetWeatherForecastAsync() =>
        await httpClient.GetFromJsonAsync<WeatherForecast[]>("/api/weather-forecasts") ??
            throw new IOException("No weather forecast!");
}
