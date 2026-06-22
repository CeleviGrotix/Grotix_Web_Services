using FluentAssertions;
using GrotixBackend.Contracts.Integration.Telemetry;
using GrotixBackend.Telemetry.Application.Internal;
using GrotixBackend.Telemetry.Application.Internal.Alerting;
using GrotixBackend.Telemetry.Domain.Model.Entities;
using GrotixBackend.Telemetry.Domain.Repositories;
using GrotixBackend.Telemetry.Domain.Services;
using Moq;
using Xunit;

namespace Telemetry.Api.Tests.Unit;

public class TelemetryDomainAndServiceTests
{
    // ==========================================
    // TDD: ReadingRangeValidator (3 Tests)
    // ==========================================
    [Fact]
    public void IsPhysicallyValid_ValueWithinRange_ReturnsTrue()
    {
        var sensor = new Sensor { MinPhysical = 0, MaxPhysical = 100 };
        var result = ReadingRangeValidator.IsPhysicallyValid(sensor, 50);
        result.Should().BeTrue();
    }

    [Fact]
    public void IsPhysicallyValid_ValueBelowMin_ReturnsFalse()
    {
        var sensor = new Sensor { MinPhysical = 0, MaxPhysical = 100 };
        var result = ReadingRangeValidator.IsPhysicallyValid(sensor, -10);
        result.Should().BeFalse();
    }

    [Fact]
    public void IsPhysicallyValid_ValueAboveMax_ReturnsFalse()
    {
        var sensor = new Sensor { MinPhysical = 0, MaxPhysical = 100 };
        var result = ReadingRangeValidator.IsPhysicallyValid(sensor, 150);
        result.Should().BeFalse();
    }

    // ==========================================
    // TDD: ThresholdEvaluator (4 Tests)
    // ==========================================
    [Theory]
    [InlineData(10, 20, 80, true)]  // Below min
    [InlineData(90, 20, 80, true)]  // Above max
    [InlineData(50, 20, 80, false)] // Inside range
    public void IsOutOfRange_EvaluatesCorrectly(double value, double min, double max, bool expected)
    {
        var result = ThresholdEvaluator.IsOutOfRange(value, min, max);
        result.Should().Be(expected);
    }

    [Fact]
    public void NearestBreachedThreshold_BelowMin_ReturnsMin()
    {
        var result = ThresholdEvaluator.NearestBreachedThreshold(10, 20, 80);
        result.Should().Be(20);
    }

    [Fact]
    public void NearestBreachedThreshold_AboveMax_ReturnsMax()
    {
        var result = ThresholdEvaluator.NearestBreachedThreshold(90, 20, 80);
        result.Should().Be(80);
    }

    [Fact]
    public void GetBreachDirection_ReturnsCorrectString()
    {
        ThresholdEvaluator.GetBreachDirection(10, 20, 80).Should().Be("BELOW_MIN");
        ThresholdEvaluator.GetBreachDirection(90, 20, 80).Should().Be("ABOVE_MAX");
    }

    // ==========================================
    // TDD: MovingAverageFilter (2 Tests)
    // ==========================================
    [Fact]
    public void Smooth_EmptyList_ReturnsNewValue()
    {
        var result = MovingAverageFilter.Smooth(new List<double>(), 25.5);
        result.Should().Be(25.5);
    }

    [Fact]
    public void Smooth_WithExistingValues_ReturnsAverage()
    {
        // Window is 3. We pass 2 elements + 1 new = 3 elements to average.
        // (10 + 20 + 30) / 3 = 20
        var result = MovingAverageFilter.Smooth(new List<double> { 10, 20 }, 30, 3);
        result.Should().Be(20);
    }

    // ==========================================
    // TDD: TelemetryIngestService (1 Test)
    // ==========================================
    [Fact]
    public async Task IngestAsync_ValidEvent_SavesReadingAndEvaluatesAlerts()
    {
        // Arrange
        var mockReadingRepo = new Mock<ISensorReadingRepository>();
        var mockSensorRepo = new Mock<ISensorRepository>();
        var mockAlertService = new Mock<IAlertEvaluationService>();

        mockSensorRepo.Setup(r => r.ListByZoneAsync(It.IsAny<int>(), default))
                      .ReturnsAsync(new List<Sensor>());

        var service = new TelemetryIngestService(mockSensorRepo.Object, mockReadingRepo.Object, mockAlertService.Object);
        var integrationEvent = new TelemetryReceivedIntegrationEvent(1, 2, 25.5, 60.0, 45.0, 1000, DateTime.UtcNow);

        // Act
        await service.IngestAsync(integrationEvent);

        // Assert
        mockReadingRepo.Verify(r => r.AddAsync(It.Is<SensorReading>(s => s.Temperature == 25.5), default), Times.Once);
    }
}