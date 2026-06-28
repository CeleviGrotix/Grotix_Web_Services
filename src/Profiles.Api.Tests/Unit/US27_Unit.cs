using FluentAssertions;
using GrotixBackend.Profiles.Domain.Model.Aggregates;
using GrotixBackend.Profiles.Domain.Model.Enums;
using Xunit;

namespace Profiles.Api.Tests.Unit;

public class US27_ContractStatusUnitTests
{
    private static Contract BuildContract(
        bool isSuspended = false,
        ContractStatus status = ContractStatus.Active,
        int daysFromNow = 30) =>
        new Contract(
            associationId: 1,
            startDate: DateTime.UtcNow.AddDays(-10),
            endDate: DateTime.UtcNow.AddDays(daysFromNow),
            status: status,
            maxZones: 5,
            maxMicrocontrollers: 3,
            totalAmount: 100,
            currency: ContractCurrency.USD,
            paymentFrequency: ContractPaymentFrequency.Monthly,
            isSuspended: isSuspended);

    // Escenario 1 — Estado del servicio

    [Fact]
    public void Contract_IsNotSuspended_WhenActiveAndNotSuspended()
    {
        var contract = BuildContract(isSuspended: false, status: ContractStatus.Active);
        contract.IsSuspended.Should().BeFalse();
        contract.Status.Should().Be(ContractStatus.Active);
    }

    [Fact]
    public void Contract_IsSuspended_WhenSuspendedFlagIsTrue()
    {
        var contract = BuildContract(isSuspended: true, status: ContractStatus.Active);
        contract.IsSuspended.Should().BeTrue();
    }

    [Fact]
    public void Contract_HasCancelledStatus_WhenCancelled()
    {
        var contract = BuildContract(isSuspended: false, status: ContractStatus.Cancelled);
        contract.Status.Should().Be(ContractStatus.Cancelled);
    }

    // Escenario 2 — Cronograma de campaña

    [Fact]
    public void Contract_HasStartDate_WhenCreated()
    {
        var contract = BuildContract();
        contract.StartDate.Should().BeCloseTo(DateTime.UtcNow.AddDays(-10), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Contract_HasEndDate_WhenCreated()
    {
        var contract = BuildContract(daysFromNow: 30);
        contract.EndDate.Should().BeCloseTo(DateTime.UtcNow.AddDays(30), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Contract_EndDateIsAfterStartDate()
    {
        var contract = BuildContract();
        contract.EndDate.Should().BeAfter(contract.StartDate);
    }

    // Escenario 3 — Periodo de descanso

    [Fact]
    public void Contract_Update_CanSuspendService()
    {
        var contract = BuildContract(isSuspended: false);
        contract.Update(null, null, null, null, true, null, null, null);
        contract.IsSuspended.Should().BeTrue();
    }

    [Fact]
    public void Contract_Update_CanReactivateService()
    {
        var contract = BuildContract(isSuspended: true);
        contract.Update(null, null, null, null, false, null, null, null);
        contract.IsSuspended.Should().BeFalse();
    }

    [Fact]
    public void Contract_Constructor_Throws_WhenEndDateBeforeStartDate()
    {
        var start = DateTime.UtcNow;
        var end = start.AddDays(-1);

        Action act = () => new Contract(1, start, end, ContractStatus.Active, 5, 3, 100,
            ContractCurrency.USD, ContractPaymentFrequency.Monthly, false);

        act.Should().Throw<ArgumentException>().WithMessage("EndDate no puede ser anterior a StartDate.");
    }

    [Fact]
    public void Contract_HasExpectedLimits_WhenCreated()
    {
        var contract = BuildContract();
        contract.MaxZones.Should().Be(5);
        contract.MaxMicrocontrollers.Should().Be(3);
    }
}