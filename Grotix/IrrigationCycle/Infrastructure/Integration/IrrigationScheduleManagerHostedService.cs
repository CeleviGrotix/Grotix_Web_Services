using System.Globalization;
using GrotixBackend.CultivationArea.Domain.Model.ValueObjects;
using GrotixBackend.IrrigationCycle.Application.Internal;
using GrotixBackend.IrrigationCycle.Application.ACL;
using GrotixBackend.IrrigationCycle.Domain.Model.Aggregates;
using GrotixBackend.IrrigationCycle.Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GrotixBackend.IrrigationCycle.Infrastructure.Integration;

/// <summary>
/// Ejecuta programas de riego (tabla <c>irrigation_schedule</c>) creando ciclos
/// automáticos cuando llega la hora configurada.
/// </summary>
public sealed class IrrigationScheduleManagerHostedService(
    IServiceProvider services,
    IOptions<WeatherForecastOptions> weatherOptions,
    ILogger<IrrigationScheduleManagerHostedService> logger) : BackgroundService
{
    // Ventana de tolerancia para disparar un riego respecto a la hora exacta del programa.
    // Si el servicio estuvo caído y se levanta dentro de esta ventana, aún se dispara.
    private static readonly TimeSpan TriggerTolerance = TimeSpan.FromMinutes(10);
    private readonly WeatherForecastOptions _weatherOptions = weatherOptions.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ExecuteIterationAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error running irrigation schedule manager");
            }

            // Frecuencia de chequeo: una vez por minuto es suficiente para riegos.
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }

    private async Task ExecuteIterationAsync(CancellationToken stoppingToken)
    {
        await using var scope = services.CreateAsyncScope();

        var scheduleRepository = scope.ServiceProvider.GetRequiredService<IIrrigationScheduleRepository>();
        var cycleRepository = scope.ServiceProvider.GetRequiredService<IIrrigationCycleRepository>();
        var commandService = scope.ServiceProvider.GetRequiredService<IIrrigationCommandService>();
        var irrigationContextService = scope.ServiceProvider.GetRequiredService<IIrrigationContextService>();
        var weatherForecastAdapter = scope.ServiceProvider.GetRequiredService<IWeatherForecastAdapter>();

        var nowUtc = DateTime.UtcNow;
        var today = nowUtc.Date;
        var nowTime = TimeOnly.FromDateTime(nowUtc);
        var todayToken = GetDayToken(nowUtc.DayOfWeek);

        IReadOnlyList<IrrigationSchedule> schedules;
        var rainForecast = await weatherForecastAdapter.GetTodayRainForecastAsync(stoppingToken);
        if (rainForecast.WillRainToday)
        {
            logger.LogInformation(
                "Rain predicted today for configured location ({Lat},{Lon}). precipitation={PrecipitationMm}mm probability={Probability}%",
                _weatherOptions.Latitude.ToString(CultureInfo.InvariantCulture),
                _weatherOptions.Longitude.ToString(CultureInfo.InvariantCulture),
                rainForecast.PrecipitationMm,
                rainForecast.PrecipitationProbabilityPercent);
        }

        try
        {
            schedules = await scheduleRepository.ListAsync(zoneId: null);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error loading irrigation schedules");
            return;
        }

        foreach (var schedule in schedules)
        {
            if (!schedule.IsActive)
                continue;

            if (!IsDayIncluded(schedule.DaysOfTheWeek, todayToken))
                continue;

            // ¿Estamos cerca de la hora programada?
            var scheduledDateTimeUtc = new DateTime(
                today.Year,
                today.Month,
                today.Day,
                schedule.StartTime.Hour,
                schedule.StartTime.Minute,
                0,
                DateTimeKind.Utc);

            var diff = nowUtc - scheduledDateTimeUtc;
            if (diff < TimeSpan.Zero || diff > TriggerTolerance)
                continue;

            // Evitar riegos duplicados: si ya hay un ciclo activo en la zona, no hacemos nada.
            var active = await cycleRepository.GetActiveByZoneAsync(schedule.ZoneId);
            if (active != null)
                continue;

            // Si ya hubo un ciclo hoy para esta zona iniciado después de la hora del programa, asumimos que ya se ejecutó.
            var history = await cycleRepository.ListHistoryAsync(
                schedule.ZoneId,
                startTime: scheduledDateTimeUtc,
                endTime: null,
                limit: 1);

            if (history.Count > 0)
                continue;

            var zoneContext = await irrigationContextService.GetZoneContextAsync(schedule.ZoneId, stoppingToken);
            if (zoneContext == null || !IrrigationModes.IsAutomatic(zoneContext.IrrigationMode))
            {
                logger.LogDebug(
                    "Skipping scheduled irrigation for zone {ZoneId}: irrigation mode is {Mode}.",
                    schedule.ZoneId,
                    zoneContext?.IrrigationMode ?? "UNKNOWN");
                continue;
            }

            if (rainForecast.WillRainToday)
            {
                var shouldSkip = await ShouldSkipByWeatherAndHumidityAsync(
                    schedule.ZoneId,
                    irrigationContextService,
                    stoppingToken);
                if (shouldSkip)
                    continue;
            }

            try
            {
                logger.LogInformation(
                    "Starting scheduled irrigation for zone {ZoneId} at {NowUtc} (schedule {ScheduleId})",
                    schedule.ZoneId,
                    nowUtc,
                    schedule.Id);

                await commandService.StartManualAsync(
                    schedule.ZoneId,
                    volumeLiters: null,
                    durationMinutes: schedule.DurationMinutes,
                    cancellationToken: stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Error starting scheduled irrigation for zone {ZoneId} (schedule {ScheduleId})",
                    schedule.ZoneId,
                    schedule.Id);
            }
        }
    }

    private async Task<bool> ShouldSkipByWeatherAndHumidityAsync(
        int zoneId,
        IIrrigationContextService irrigationContextService,
        CancellationToken cancellationToken)
    {
        var context = await irrigationContextService.GetZoneContextAsync(zoneId, cancellationToken);
        if (context == null)
        {
            logger.LogInformation(
                "Skipping scheduled irrigation for zone {ZoneId}: rain expected and no context available.",
                zoneId);
            return true;
        }

        if (!context.CurrentHumiditySoilPercent.HasValue)
        {
            logger.LogInformation(
                "Skipping scheduled irrigation for zone {ZoneId}: rain expected and no current humidity.",
                zoneId);
            return true;
        }

        var humidityDeficit = context.OptimalHumiditySoil - context.CurrentHumiditySoilPercent.Value;
        if (humidityDeficit <= _weatherOptions.HumidityDeficitThresholdPercent)
        {
            logger.LogInformation(
                "Skipping scheduled irrigation for zone {ZoneId}: rain expected and humidity deficit {Deficit:F2}% is below threshold {Threshold:F2}%.",
                zoneId,
                humidityDeficit,
                _weatherOptions.HumidityDeficitThresholdPercent);
            return true;
        }

        logger.LogInformation(
            "Rain expected but irrigation kept for zone {ZoneId}: humidity deficit {Deficit:F2}% exceeds threshold {Threshold:F2}%.",
            zoneId,
            humidityDeficit,
            _weatherOptions.HumidityDeficitThresholdPercent);
        return false;
    }

    private static bool IsDayIncluded(string daysOfTheWeek, string todayToken)
    {
        if (string.IsNullOrWhiteSpace(daysOfTheWeek))
            return false;

        var tokens = daysOfTheWeek
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(d => d.Trim().ToUpperInvariant());

        return tokens.Contains(todayToken);
    }

    private static string GetDayToken(DayOfWeek dayOfWeek) =>
        dayOfWeek switch
        {
            DayOfWeek.Monday => "MON",
            DayOfWeek.Tuesday => "TUE",
            DayOfWeek.Wednesday => "WED",
            DayOfWeek.Thursday => "THU",
            DayOfWeek.Friday => "FRI",
            DayOfWeek.Saturday => "SAT",
            DayOfWeek.Sunday => "SUN",
            _ => throw new ArgumentOutOfRangeException(nameof(dayOfWeek), dayOfWeek, null)
        };
}

