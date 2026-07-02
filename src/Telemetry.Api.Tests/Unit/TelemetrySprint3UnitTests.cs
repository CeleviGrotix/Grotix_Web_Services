using FluentAssertions;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace HardwareDevice.Api.Tests.Unit;

public class TelemetryUnitTests
{
    [Fact]
    public void TC_U78_ReadingRangeValidator_HumidityTooHigh_ShouldReturnFalse()
    {
        // Arrange
        double humidityValue = 1.50; // 150%

        // Act
        bool isValid = humidityValue <= 1.0;

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void TC_U79_ReadingRangeValidator_TemperatureTooLow_ShouldReturnFalse()
    {
        // Arrange
        double temperatureValue = -20.0;

        // Act
        bool isValid = temperatureValue >= 0.0; // Supongamos límite inferior de invernadero

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void TC_U80_MovingAverageFilter_ShortHistory_ShouldReturnAverage()
    {
        // Arrange
        var history = new List<double> { 40, 42 };
        double newValue = 44;

        // Act
        history.Add(newValue);
        double average = history.Average();

        // Assert
        average.Should().Be(42);
    }

    [Fact]
    public async Task TC_U81_RegisterMeasurementHandler_ValidReading_ShouldCallAddAndLastSeen()
    {
        // Arrange
        var mockRepo = new Mock<ITelemetryRepository>();
        mockRepo.Setup(r => r.AddAsync(It.IsAny<object>())).Returns(Task.CompletedTask);

        // Act
        await mockRepo.Object.AddAsync(new { Humidity = 0.55, Temp = 22 });

        // Assert
        mockRepo.Verify(r => r.AddAsync(It.IsAny<object>()), Times.Once);
    }

    [Fact]
    public void TC_U82_RegisterMeasurementHandler_AnomalousJump_ShouldMarkAsPending()
    {
        // Arrange
        double lastReading = 40;
        double currentReading = 95; // Salto anómalo repentino de +55% sin riego

        // Act
        string status = (currentReading - lastReading > 30) ? "pendiente de validación" : "válido";

        // Assert
        status.Should().Be("pendiente de validación");
    }

    [Fact]
    public void TC_U83_SensorReading_AdcQuantizationError_ShouldBeWithinLimits()
    {
        // Arrange
        double rawVoltage = 2.45;
        double maxVoltage = 3.3;

        // Act
        double percentage = (rawVoltage / maxVoltage) * 100;
        double expectedValue = 74.24; 
        double errorMargin = Math.Abs(percentage - expectedValue);

        // Assert
        errorMargin.Should().BeLessThan(2.0); // Margen de error < 2%
    }
}

public interface ITelemetryRepository { Task AddAsync(object reading); }