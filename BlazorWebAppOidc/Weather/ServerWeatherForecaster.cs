using BlazorWebAppOidc.Client.Weather;

namespace BlazorWebAppOidc.Weather;

internal sealed class ServerWeatherForecaster(IHttpClientFactory clientFactory) : IWeatherForecaster
{
    private const string ClientName = "ExternalApi";
    private const string Endpoint = "/api/weather-forecasts";

    public async Task<IEnumerable<WeatherForecast>> GetWeatherForecastAsync()
    {
        try
        {
            var client = clientFactory.CreateClient(ClientName);

            using var response = await client.GetAsync(Endpoint);
            response.EnsureSuccessStatusCode();

            var forecasts = await response.Content.ReadFromJsonAsync<WeatherForecast[]>();

            return forecasts ?? throw new IOException("No weather forecast data was returned.");
        }
        catch (HttpRequestException ex)
        {
            throw new IOException("Failed to retrieve weather forecast due to a network error.", ex);
        }
        catch (TaskCanceledException ex)
        {
            throw new IOException("The request for weather forecast timed out.", ex);
        }
    }
}
