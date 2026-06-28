using FluentAssertions;
using GrotixBackend.CultivationArea.Domain.Model.Aggregates;
using Xunit;

namespace CultivationArea.Api.Tests.Unit;

/// <summary>
/// US23 — Clasificación del estado fenológico mediante IA
/// Unit tests: validan el dominio de AnalysisReport.
/// </summary>
public class US23_AiClassificationUnitTests
{
    // Escenario 1 — Categorización exitosa

    [Fact]
    public void AnalysisReport_Constructor_SetsAllFields_WhenValidDataProvided()
    {
        var report = new AnalysisReport(1, "Germinacion", 85f);

        report.ZoneId.Should().Be(1);
        report.DetectedPhase.Should().Be("Germinacion");
        report.HealthScore.Should().Be(85f);
        report.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Theory]
    [InlineData("Semilla")]
    [InlineData("Germinacion")]
    [InlineData("Vegetativo")]
    [InlineData("Floracion")]
    [InlineData("Fructificacion")]
    [InlineData("Cosecha")]
    public void AnalysisReport_AcceptsAllValidPhases(string phase)
    {
        var report = new AnalysisReport(1, phase, 80f);
        report.DetectedPhase.Should().Be(phase);
    }

    // Escenario 2 — Umbral de confianza (75%)

    [Fact]
    public void AnalysisReport_AcceptsIndeterminadoPhase_WhenConfidenceBelowThreshold()
    {
        var report = new AnalysisReport(1, "Indeterminado", 50f);
        report.DetectedPhase.Should().Be("Indeterminado");
        report.HealthScore.Should().Be(50f);
    }

    [Fact]
    public void AnalysisReport_AcceptsHealthScore_AtExactly75()
    {
        var report = new AnalysisReport(1, "Germinacion", 75f);
        report.HealthScore.Should().Be(75f);
    }

    // Escenario 3 — Validaciones del dominio

    [Fact]
    public void AnalysisReport_Constructor_Throws_WhenZoneIdIsInvalid()
    {
        Action act = () => new AnalysisReport(0, "Germinacion", 80f);
        act.Should().Throw<ArgumentException>().WithMessage("ZoneId inválido.");
    }

    [Fact]
    public void AnalysisReport_Constructor_Throws_WhenDetectedPhaseIsEmpty()
    {
        Action act = () => new AnalysisReport(1, "", 80f);
        act.Should().Throw<ArgumentException>().WithMessage("DetectedPhase requerido.");
    }

    [Fact]
    public void AnalysisReport_Constructor_Throws_WhenHealthScoreExceeds100()
    {
        Action act = () => new AnalysisReport(1, "Germinacion", 101f);
        act.Should().Throw<ArgumentException>().WithMessage("HealthScore debe estar entre 0 y 100.");
    }

    [Fact]
    public void AnalysisReport_Constructor_Throws_WhenHealthScoreIsNegative()
    {
        Action act = () => new AnalysisReport(1, "Germinacion", -1f);
        act.Should().Throw<ArgumentException>().WithMessage("HealthScore debe estar entre 0 y 100.");
    }

    [Fact]
    public void AnalysisReport_TrimsWhitespace_FromDetectedPhase()
    {
        var report = new AnalysisReport(1, "  Germinacion  ", 80f);
        report.DetectedPhase.Should().Be("Germinacion");
    }
}