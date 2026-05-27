using System.Globalization;
using System.Text.Json;
using GrotixBackend.IrrigationCycle.Application.ACL;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GrotixBackend.IrrigationCycle.Infrastructure.Integration;

public sealed class OpenMeteoWeatherForecastAdapter(
    HttpClient httpClient,
    IOptions<WeatherForecastOptions> options,
    ILogger<OpenMeteoWeatherForecastAdapter> logger) : IWeatherForecastAdapter
{
    private readonly WeatherForecastOptions _options = options.Value;

    public async Task<DailyRainForecast> GetTodayRainForecastAsync(CancellationToken cancellationToken = default)
    {
        // InvariantCulture: en es-ES el interpolado de double usa coma y Open-Meteo devuelve 400.
        var lat = _options.Latitude.ToString(CultureInfo.InvariantCulture);
        var lon = _options.Longitude.ToString(CultureInfo.InvariantCulture);
        var url =
            $"{_options.BaseUrl}?latitude={lat}&longitude={lon}" +
            "&daily=precipitation_sum,precipitation_probability_max&forecast_days=1&timezone=UTC";

        try
        {
            using var response = await httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

            var daily = document.RootElement.GetProperty("daily");
            var precipitationMm = daily.GetProperty("precipitation_sum")[0].GetDouble();
            var probabilityPercent = daily.TryGetProperty("precipitation_probability_max", out var probabilityArray)
                ? probabilityArray[0].GetInt32()
                : 0;

            var willRain = precipitationMm >= _options.RainMmThreshold ||
                           probabilityPercent >= _options.RainProbabilityThresholdPercent;

            return new DailyRainForecast(willRain, precipitationMm, probabilityPercent);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Weather forecast lookup failed. Fallback: no rain predicted.");
            return new DailyRainForecast(false, 0, 0);
        }
    }
}

