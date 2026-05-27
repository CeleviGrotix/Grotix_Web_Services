namespace GrotixBackend.IrrigationCycle.Application.ACL;

public interface IWeatherForecastAdapter
{
    Task<DailyRainForecast> GetTodayRainForecastAsync(CancellationToken cancellationToken = default);
}

public sealed record DailyRainForecast(
    bool WillRainToday,
    double PrecipitationMm,
    int PrecipitationProbabilityPercent);

public sealed class WeatherForecastOptions
{
    public string BaseUrl { get; set; } = "https://api.open-meteo.com/v1/forecast";
    public double Latitude { get; set; } = -12.0464; // Lima
    public double Longitude { get; set; } = -77.0428; // Lima
    public double RainMmThreshold { get; set; } = 0.5;
    public int RainProbabilityThresholdPercent { get; set; } = 50;
    public double HumidityDeficitThresholdPercent { get; set; } = 5.0;
}

