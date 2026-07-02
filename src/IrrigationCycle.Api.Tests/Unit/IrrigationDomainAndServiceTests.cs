using FluentAssertions;
using GrotixBackend.IrrigationCycle.Application.ACL;
using GrotixBackend.IrrigationCycle.Application.Internal;
using GrotixBackend.IrrigationCycle.Domain.Model.Aggregates;
using GrotixBackend.IrrigationCycle.Domain.Model.ValueObjects;
using GrotixBackend.IrrigationCycle.Domain.Repositories;
using GrotixBackend.IrrigationCycle.Domain.Services;
using Moq;
using Xunit;

namespace IrrigationCycle.Api.Tests.Unit;

public class IrrigationDomainAndServiceTests
{
    // ==========================================
    // TDD: IrrigationCalculator (5 Tests)
    // ==========================================
    [Fact]
    public void CalculateVolumeLiters_NullInputs_ReturnsDefaultVolume()
    {
        var volume = IrrigationCalculator.CalculateVolumeLiters(null, 60.0);
        volume.Should().Be(25); // DefaultVolumeLiters
    }

    [Fact]
    public void CalculateVolumeLiters_CurrentGreaterOrEqualTarget_ReturnsDefaultVolume()
    {
        var volume = IrrigationCalculator.CalculateVolumeLiters(65.0, 60.0);
        volume.Should().Be(25); // Deficit <= 0 -> DefaultVolumeLiters
    }

    [Fact]
    public void CalculateVolumeLiters_ValidDeficit_CalculatesCorrectly()
    {
        // Target = 60, Current = 40 => Deficit = 20.
        // 20 * LitersPerHumidityPoint (2.5) = 50 Litros.
        var volume = IrrigationCalculator.CalculateVolumeLiters(40.0, 60.0);
        volume.Should().Be(50);
    }

    [Fact]
    public void EstimateDurationMinutes_DividesByFlowRateAndRoundsUp()
    {
        // 12 Liters / FlowLitersPerMinute (5) = 2.4 => Ceiling = 3.
        var duration = IrrigationCalculator.EstimateDurationMinutes(12);
        duration.Should().Be(3);
    }

    [Fact]
    public void ResolveDurationMinutes_RequestedProvided_ReturnsRequested()
    {
        var duration = IrrigationCalculator.ResolveDurationMinutes(50, 15);
        duration.Should().Be(15);
    }

    // ==========================================
    // TDD: IrrigationCycleRecord Aggregate (4 Tests)
    // ==========================================
    [Fact]
    public void Constructor_ValidData_SetsPropertiesAndStatusInProgress()
    {
        var cycle = new IrrigationCycleRecord(1, 50.0, 10);
        cycle.ZoneId.Should().Be(1);
        cycle.VolumeLiters.Should().Be(50.0);
        cycle.Status.Should().Be(CycleStatuses.InProgress);
    }

    [Fact]
    public void Constructor_NegativeVolume_ThrowsArgumentException()
    {
        Action act = () => new IrrigationCycleRecord(1, -10, 10);
        act.Should().Throw<ArgumentException>().WithMessage("VolumeLiters debe ser positivo.");
    }

    [Fact]
    public void Complete_UpdatesStatusAndSetsEndTime()
    {
        var cycle = new IrrigationCycleRecord(1, 50.0, 10);
        cycle.Complete(45.0); // Override actual volume

        cycle.Status.Should().Be(CycleStatuses.Completed);
        cycle.EndTime.Should().NotBeNull();
        cycle.VolumeLiters.Should().Be(45.0);
    }

    [Fact]
    public void Abort_UpdatesStatusAndSetsReason()
    {
        var cycle = new IrrigationCycleRecord(1, 50.0, 10);
        cycle.Abort("MANUAL_CANCEL");

        cycle.Status.Should().Be(CycleStatuses.Aborted);
        cycle.EndTime.Should().NotBeNull();
        cycle.AbortReason.Should().Be("MANUAL_CANCEL");
    }

    // ==========================================
    // TDD: IrrigationCommandService (1 Test)
    // ==========================================
    [Fact]
    public async Task StartManualAsync_ZoneDoesNotExist_ThrowsArgumentException()
    {
        // Arrange
        var mockZoneAccess = new Mock<IZoneAccessService>();
        mockZoneAccess.Setup(s => s.ZoneExistsAsync(It.IsAny<int>())).ReturnsAsync(false);
        
        var service = new IrrigationCommandService(
            new Mock<IIrrigationCycleRepository>().Object,
            new Mock<IIrrigationUnitOfWork>().Object,
            mockZoneAccess.Object,
            new Mock<IIrrigationContextService>().Object,
            new Mock<IActuatorControlService>().Object,
            new Mock<IIrrigationCompletedPublisher>().Object);

        // Act
        Func<Task> act = async () => await service.StartManualAsync(99, 50, 10);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("La zona no existe.");
    }
}