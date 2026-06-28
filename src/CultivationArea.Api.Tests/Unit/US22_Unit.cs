using FluentAssertions;
using GrotixBackend.CultivationArea.Domain.Model.Aggregates;
using GrotixBackend.CultivationArea.Domain.Model.Commands;
using Xunit;

namespace CultivationArea.Api.Tests.Unit;

/// <summary>
/// US22 — Gestión de registro fotográfico de cultivos
/// El "registro visual" es una URL de imagen guardada en la zona.
/// </summary>
public class US22_ZoneImageUrlUnitTests
{
    private static Zone BuildZone(string? imageUrl = null) =>
        new Zone(
            farmId: 1,
            cropId: 1,
            name: "Zona Test",
            latitude: -12.0,
            longitude: -77.0,
            imageUrl: imageUrl);

    // Escenario 3 — Persistencia de la URL de imagen

    [Fact]
    public void UpdateImageUrl_PersistsUrl_WhenValidHttpsUrlProvided()
    {
        var zone = BuildZone();
        const string url = "https://storage.azure.com/grotix/cultivo.jpg";

        zone.UpdateImageUrl(url);

        zone.ImageUrl.Should().Be(url);
    }

    [Fact]
    public void UpdateImageUrl_PersistsUrl_WhenValidHttpUrlProvided()
    {
        var zone = BuildZone();
        const string url = "http://cdn.grotix.com/zones/photo.png";

        zone.UpdateImageUrl(url);

        zone.ImageUrl.Should().Be(url);
    }

    // Escenario 2 — Validación de formato (dominio)

    [Fact]
    public void UpdateImageUrl_SetsNull_WhenEmptyStringProvided()
    {
        var zone = BuildZone("https://storage.azure.com/grotix/old.jpg");

        zone.UpdateImageUrl("");

        zone.ImageUrl.Should().BeNull();
    }

    [Fact]
    public void UpdateImageUrl_SetsNull_WhenWhitespaceProvided()
    {
        var zone = BuildZone("https://storage.azure.com/grotix/old.jpg");

        zone.UpdateImageUrl("   ");

        zone.ImageUrl.Should().BeNull();
    }

    [Fact]
    public void UpdateImageUrl_TrimsWhitespace_WhenUrlHasLeadingTrailingSpaces()
    {
        var zone = BuildZone();
        const string url = "https://storage.azure.com/grotix/cultivo.jpg";

        zone.UpdateImageUrl($"  {url}  ");

        zone.ImageUrl.Should().Be(url);
    }

    [Fact]
    public void UpdateImageUrl_ClearsImage_WhenNullProvided()
    {
        var zone = BuildZone("https://storage.azure.com/grotix/old.jpg");

        zone.UpdateImageUrl(null);

        zone.ImageUrl.Should().BeNull();
    }

    // Escenario 3 — No afecta otros campos al actualizar imagen

    [Fact]
    public void UpdateImageUrl_DoesNotModifyOtherZoneFields()
    {
        var zone = BuildZone();

        zone.UpdateImageUrl("https://storage.azure.com/grotix/cultivo.jpg");

        zone.Name.Should().Be("Zona Test");
        zone.FarmId.Should().Be(1);
        zone.CropId.Should().Be(1);
    }

    // Comando

    [Fact]
    public void UpdateZoneCommand_CarriesImageUrl_ForPersistence()
    {
        const string url = "https://storage.azure.com/grotix/cultivo.jpg";

        var command = new UpdateZoneCommand(
            ZoneId: 1,
            Name: null, CropId: null, Latitude: null, Longitude: null,
            CurrentPhase: null, PhaseStartDate: null,
            ImageUrl: url,
            IrrigationMode: null);

        command.ImageUrl.Should().Be(url);
        command.ZoneId.Should().Be(1);
    }

    [Fact]
    public void UpdateZoneCommand_AllowsNull_ToRemoveImage()
    {
        var command = new UpdateZoneCommand(
            ZoneId: 1,
            Name: null, CropId: null, Latitude: null, Longitude: null,
            CurrentPhase: null, PhaseStartDate: null,
            ImageUrl: null,
            IrrigationMode: null);

        command.ImageUrl.Should().BeNull();
    }
}