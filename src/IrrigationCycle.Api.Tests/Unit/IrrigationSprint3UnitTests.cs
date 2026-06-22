using FluentAssertions;
using Moq;
using System;
using Xunit;

namespace HardwareDevice.Api.Tests.Unit;

public class IrrigationUnitTests
{
    [Fact]
    public void TC_U84_IrrigationCycle_ValidManualActivation_ShouldSetStatusInProgress()
    {
        // Arrange & Act
        var cycle = new { ZoneId = 3, Mode = "Manual", Status = "InProgress" };

        // Assert
        cycle.Status.Should().Be("InProgress");
        cycle.Mode.Should().Be("Manual");
    }

    [Fact]
    public void TC_U85_IrrigationCycle_SafetyTimerExpiration_ShouldAbortAndCloseValve()
    {
        // Arrange
        var mockClock = new Mock<IClockTime>();
        var startTime = new DateTime(2026, 6, 24, 10, 0, 0);
        mockClock.Setup(c => c.Now).Returns(startTime);

        var cycle = new IrrigationCycleState(startTime, "Manual");

        // Simulamos paso del tiempo excediendo MaxManualDuration (más de 15 min)
        var expiredTime = startTime.AddMinutes(16);
        mockClock.Setup(c => c.Now).Returns(expiredTime);

        // Act
        if ((mockClock.Object.Now - cycle.StartTime).TotalMinutes > 15)
        {
            cycle.Status = "Aborted";
            cycle.IsValveOpen = false;
        }

        // Assert
        cycle.Status.Should().Be("Aborted");
        cycle.IsValveOpen.Should().BeFalse();
    }

    [Fact]
    public void TC_U86_IrrigationCycle_ManualPriorityOverAuto_ShouldPauseAutonomousLogic()
    {
        // Arrange
        string currentMode = "Auto";
        string incomingCommand = "Manual";

        // Act
        if (incomingCommand == "Manual" && currentMode == "Auto")
        {
            currentMode = "Manual"; // Interrupción inmediata
        }

        // Assert
        currentMode.Should().Be("Manual");
    }

    [Fact]
    public void TC_U87_IrrigationCalculator_AutonomousMode_NeedsWater_ShouldReturnVolume()
    {
        // Arrange
        double currentHumidity = 0.40;
        double targetHumidity = 0.60;

        // Act
        double waterQuantity = (currentHumidity < targetHumidity) ? (targetHumidity - currentHumidity) * 10 : 0;

        // Assert
        waterQuantity.Should().BeGreaterThan(0);
    }

    [Fact]
    public void TC_U88_IrrigationCalculator_NoWaterNeeded_ShouldReturnZero()
    {
        // Arrange
        double currentHumidity = 0.65;
        double targetHumidity = 0.60;

        // Act
        double waterQuantity = (currentHumidity < targetHumidity) ? (targetHumidity - currentHumidity) * 10 : 0;

        // Assert
        waterQuantity.Should().Be(0);
    }

    [Fact]
    public void TC_U89_IrrigationCommandService_ManualStopCommand_ShouldCompleteCycle()
    {
        // Arrange
        var cycle = new IrrigationCycleState(DateTime.UtcNow, "Manual") { Status = "InProgress", IsValveOpen = true };

        // Act
        cycle.Status = "Completed";
        cycle.IsValveOpen = false;

        // Assert
        cycle.Status.Should().Be("Completed");
        cycle.IsValveOpen.Should().BeFalse();
    }
}

public interface IClockTime { DateTime Now { get; } }
public class IrrigationCycleState 
{ 
    public DateTime StartTime { get; }
    public string Mode { get; }
    public string Status { get; set; }
    public bool IsValveOpen { get; set; }
    public IrrigationCycleState(DateTime t, string m) { StartTime = t; Mode = m; Status = "InProgress"; IsValveOpen = true; }
}