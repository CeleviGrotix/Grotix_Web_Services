using FluentAssertions;
using GrotixBackend.Contracts.Profiles.Access;
using GrotixBackend.CultivationArea.Application.Internal.CommandServices;
using GrotixBackend.CultivationArea.Domain.Model.Aggregates;
using GrotixBackend.CultivationArea.Domain.Model.Commands;
using GrotixBackend.CultivationArea.Domain.Repositories;
using Moq;
using Xunit;

namespace CultivationArea.Api.Tests.Unit;

public class CultivationDomainAndServiceTests
{
    // ── Farm Aggregate ────────────────────────────────────────────────────────

    [Fact]
    public void FarmConstructor_InvalidAssociation_ThrowsArgumentException()
    {
        Action act = () => new Farm(1, 0, "Granja Central", "Valle Norte");
        act.Should().Throw<ArgumentException>().WithMessage("AssociationId inválido.");
    }

    [Fact]
    public void FarmConstructor_EmptyName_ThrowsArgumentException()
    {
        Action act = () => new Farm(1, 10, "", "Valle Norte");
        act.Should().Throw<ArgumentException>().WithMessage("El nombre de la granja no puede estar vacío.");
    }

    [Fact]
    public void FarmUpdate_ValidData_UpdatesProperties()
    {
        var farm = new Farm(1, 10, "Granja Norte", "Ubicacion 1");
        farm.Update("Granja Sur", "Ubicacion 2");
        farm.Name.Should().Be("Granja Sur");
        farm.Location.Should().Be("Ubicacion 2");
    }

    // ── Zone Aggregate ────────────────────────────────────────────────────────

    [Fact]
    public void ZoneConstructor_InvalidFarmOrCrop_ThrowsArgumentException()
    {
        Action act = () => new Zone(0, 5, "Zona Test", 12.34, -56.78);
        act.Should().Throw<ArgumentException>().WithMessage("FarmId y CropId deben ser válidos.");
    }

    [Fact]
    public void ZoneUpdateCoordinates_ValidData_UpdatesLatAndLng()
    {
        var zone = new Zone(1, 1, "Zona Test", 10.0, 10.0);
        zone.UpdateCoordinates(20.5, -30.5);
        zone.Latitude.Should().Be(20.5);
        zone.Longitude.Should().Be(-30.5);
    }

    // ── FarmCommandService ────────────────────────────────────────────────────

    [Fact]
    public async Task CreateFarm_AssociationDoesNotExist_ThrowsArgumentException()
    {
        var mockAssocExistence = new Mock<IAssociationExistenceService>();
        mockAssocExistence.Setup(s => s.ExistsAsync(It.IsAny<int>())).ReturnsAsync(false);

        var service = new FarmCommandService(
            new Mock<IFarmRepository>().Object,
            mockAssocExistence.Object,
            new Mock<IAssociationOwnerLookupService>().Object,
            new Mock<ICultivationAreaUnitOfWork>().Object);

        Func<Task> act = async () => await service.Handle(new CreateFarmCommand(99, "Granja", "Lima"));
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("La asociación 99 no existe.");
    }

    [Fact]
    public async Task CreateFarm_NameAlreadyExists_ThrowsArgumentException()
    {
        var mockAssocExistence = new Mock<IAssociationExistenceService>();
        mockAssocExistence.Setup(s => s.ExistsAsync(It.IsAny<int>())).ReturnsAsync(true);

        var mockFarmRepo = new Mock<IFarmRepository>();
        mockFarmRepo.Setup(r => r.ExistsByAssociationAndNameAsync(It.IsAny<int>(), It.IsAny<string>(), null)).ReturnsAsync(true);

        var service = new FarmCommandService(
            mockFarmRepo.Object,
            mockAssocExistence.Object,
            new Mock<IAssociationOwnerLookupService>().Object,
            new Mock<ICultivationAreaUnitOfWork>().Object);

        Func<Task> act = async () => await service.Handle(new CreateFarmCommand(10, "Granja Duplicada", "Lima"));
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("Ya existe una granja con ese nombre en la organización.");
    }

    [Fact]
    public async Task CreateFarm_ValidData_ReturnsCreatedFarm()
    {
        var mockAssocExistence = new Mock<IAssociationExistenceService>();
        mockAssocExistence.Setup(s => s.ExistsAsync(It.IsAny<int>())).ReturnsAsync(true);

        var service = new FarmCommandService(
            new Mock<IFarmRepository>().Object,
            mockAssocExistence.Object,
            new Mock<IAssociationOwnerLookupService>().Object,
            new Mock<ICultivationAreaUnitOfWork>().Object);

        var result = await service.Handle(new CreateFarmCommand(10, "Granja Nueva", "Lima"));
        result.Should().NotBeNull();
        result.Name.Should().Be("Granja Nueva");
    }

    // ── ZoneCommandService ────────────────────────────────────────────────────

    [Fact]
    public async Task CreateZone_CropDoesNotExist_ThrowsArgumentException()
    {
        var mockCropRepo = new Mock<ICropRepository>();
        mockCropRepo.Setup(r => r.ExistsAsync(It.IsAny<int>())).ReturnsAsync(false);

        var service = new ZoneCommandService(
            new Mock<IZoneRepository>().Object,
            mockCropRepo.Object,
            new Mock<ICultivationAreaUnitOfWork>().Object);

        Func<Task> act = async () => await service.Handle(
            new CreateZoneCommand(1, 99, "Zona Test", 10.0, 10.0, null, null, null));
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("El cultivo 99 no existe.");
    }

    [Fact]
    public async Task UpdateZone_CropDoesNotExist_ThrowsArgumentException()
    {
        var mockZoneRepo = new Mock<IZoneRepository>();
        mockZoneRepo.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new Zone(1, 2, "Zona Test", 10.0, 10.0));

        var mockCropRepo = new Mock<ICropRepository>();
        mockCropRepo.Setup(r => r.ExistsAsync(99)).ReturnsAsync(false);

        var service = new ZoneCommandService(
            mockZoneRepo.Object,
            mockCropRepo.Object,
            new Mock<ICultivationAreaUnitOfWork>().Object);

        Func<Task> act = async () => await service.Handle(
            new UpdateZoneCommand(1, null, 99, null, null, null, null, null));
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("El cultivo 99 no existe.");
    }
}