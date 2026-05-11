using Xunit;
using Microsoft.EntityFrameworkCore;
using GrotixBackend.Shared.Infrastructure.Persistence.EFC.Configuration;
using GrotixBackend.Shared.Domain.Repositories;
using GrotixBackend.CultivationArea.Application.Internal.CommandServices;
using GrotixBackend.CultivationArea.Domain.Model.Commands;
using GrotixBackend.CultivationArea.Domain.Repositories;
using GrotixBackend.CultivationArea.Domain.Model.Aggregates;
using GrotixBackend.Profiles.Domain.Model.Aggregates;
using GrotixBackend.Profiles.Domain.Model.ValueObjects;

namespace Grotix.UnitTests.Integration;

// ── FAKES ─────────────────────────────────────────────────────────────────────

file class FakeUnitOfWork(AppDbContext context) : IUnitOfWork
{
    public async Task CompleteAsync() => await context.SaveChangesAsync();
}

file class FakeFarmRepository(AppDbContext context) : IFarmRepository
{
    public async Task<Farm?> GetByIdAsync(int id) => await context.Farms.FindAsync(id);
    public async Task<IEnumerable<Farm>> ListAsync() => await context.Farms.ToListAsync();
    public async Task AddAsync(Farm entity) => await context.Farms.AddAsync(entity);
    public Task DeleteAsync(Farm entity) { context.Farms.Remove(entity); return Task.CompletedTask; }
    public async Task<bool> ExistsAsync(int id) => await context.Farms.AnyAsync(f => f.Id == id);
    public async Task<IReadOnlyList<Farm>> ListByUserIdAsync(int userId) =>
        await context.Farms.Where(f => f.UserId == userId).ToListAsync();
}

file class FakeZoneRepository(AppDbContext context) : IZoneRepository
{
    public async Task<Zone?> GetByIdAsync(int id) => await context.Zones.FindAsync(id);
    public async Task<IEnumerable<Zone>> ListAsync() => await context.Zones.ToListAsync();
    public async Task AddAsync(Zone entity) => await context.Zones.AddAsync(entity);
    public Task DeleteAsync(Zone entity) { context.Zones.Remove(entity); return Task.CompletedTask; }
    public async Task<bool> ExistsAsync(int id) => await context.Zones.AnyAsync(z => z.Id == id);
    public async Task<IReadOnlyList<Zone>> ListByFarmIdAsync(int farmId) =>
        await context.Zones.Where(z => z.FarmId == farmId).ToListAsync();
    public async Task<bool> AnyByCropIdAsync(int cropId, CancellationToken ct = default) =>
        await context.Zones.AnyAsync(z => z.CropId == cropId, ct);
}

file class FakeCropRepository(AppDbContext context) : ICropRepository
{
    public async Task<Crop?> GetByIdAsync(int id) => await context.Crops.FindAsync(id);
    public async Task<IEnumerable<Crop>> ListAsync() => await context.Crops.ToListAsync();
    public async Task AddAsync(Crop entity) => await context.Crops.AddAsync(entity);
    public Task DeleteAsync(Crop entity) { context.Crops.Remove(entity); return Task.CompletedTask; }
    public async Task<bool> ExistsAsync(int id) => await context.Crops.AnyAsync(c => c.Id == id);
}

// ── INTEGRATION TESTS ─────────────────────────────────────────────────────────

/// <summary>
/// Integration tests: Service → Repository Fake → AppDbContext (InMemory).
/// No requieren conexión a base de datos real ni red.
/// </summary>
public class FarmIntegrationTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly FarmCommandService _farmService;
    private readonly ZoneCommandService _zoneService;

    public FarmIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);

        var unitOfWork = new FakeUnitOfWork(_context);
        var farmRepo = new FakeFarmRepository(_context);
        var zoneRepo = new FakeZoneRepository(_context);
        var cropRepo = new FakeCropRepository(_context);

        _farmService = new FarmCommandService(farmRepo, unitOfWork);
        _zoneService = new ZoneCommandService(zoneRepo, cropRepo, unitOfWork);
    }

    private async Task<User> SeedUserAsync()
    {
        var email = UserEmail.Create($"farmer_{Guid.NewGuid():N}@grotix.pe");
        var user = new User(identityId: 1, email: email, roleId: 4);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    private async Task<Crop> SeedCropAsync()
    {
        var crop = new Crop("Papa", "Solanum tuberosum", 18.0, 75.0, 6.0, 48);
        _context.Crops.Add(crop);
        await _context.SaveChangesAsync();
        return crop;
    }

    // ── FARM TESTS ────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateFarm_ValidCommand_PersistsInDatabase()
    {
        var user = await SeedUserAsync();
        var farm = await _farmService.Handle(new CreateFarmCommand(user.Id, "Granja Andina", "Cusco"));

        Assert.True(farm.Id > 0);
        var persisted = await _context.Farms.FindAsync(farm.Id);
        Assert.NotNull(persisted);
        Assert.Equal("Granja Andina", persisted.Name);
    }

    [Fact]
    public async Task CreateFarm_EmptyName_ThrowsArgumentException()
    {
        var user = await SeedUserAsync();
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _farmService.Handle(new CreateFarmCommand(user.Id, "  ", "Cusco")));
    }

    [Fact]
    public async Task UpdateFarm_ValidData_UpdatesCorrectlyInDatabase()
    {
        var user = await SeedUserAsync();
        var farm = await _farmService.Handle(new CreateFarmCommand(user.Id, "Nombre Viejo", "Lima"));
        var updated = await _farmService.Handle(new UpdateFarmCommand(farm.Id, "Nombre Nuevo", "Arequipa"));

        Assert.Equal("Nombre Nuevo", updated.Name);
        var persisted = await _context.Farms.FindAsync(farm.Id);
        Assert.Equal("Nombre Nuevo", persisted!.Name);
    }

    [Fact]
    public async Task UpdateFarm_NonExistentId_ThrowsKeyNotFoundException()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _farmService.Handle(new UpdateFarmCommand(99999, "Nombre", "Lugar")));
    }

    [Fact]
    public async Task CreateMultipleFarms_SameUser_AllPersistedCorrectly()
    {
        var user = await SeedUserAsync();
        await _farmService.Handle(new CreateFarmCommand(user.Id, "Granja 1", "Lima"));
        await _farmService.Handle(new CreateFarmCommand(user.Id, "Granja 2", "Cusco"));
        await _farmService.Handle(new CreateFarmCommand(user.Id, "Granja 3", "Arequipa"));

        var farms = await _context.Farms.Where(f => f.UserId == user.Id).ToListAsync();
        Assert.Equal(3, farms.Count);
    }

    // ── ZONE TESTS ────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateZone_ValidCommand_PersistsInDatabase()
    {
        var user = await SeedUserAsync();
        var farm = await _farmService.Handle(new CreateFarmCommand(user.Id, "Granja Test", "Lima"));
        var crop = await SeedCropAsync();

        var zone = await _zoneService.Handle(
            new CreateZoneCommand(farm.Id, crop.Id, -12.046, -77.042, "Germinación", null, null));

        Assert.True(zone.Id > 0);
        var persisted = await _context.Zones.FindAsync(zone.Id);
        Assert.NotNull(persisted);
        Assert.Equal("Germinación", persisted.CurrentPhase);
    }

    [Fact]
    public async Task CreateZone_NonExistentCrop_ThrowsArgumentException()
    {
        var user = await SeedUserAsync();
        var farm = await _farmService.Handle(new CreateFarmCommand(user.Id, "Granja Test", "Lima"));

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _zoneService.Handle(new CreateZoneCommand(farm.Id, 99999, -12.046, -77.042, null, null, null)));
    }

    [Fact]
    public async Task UpdateZone_ValidPhase_UpdatesPhaseInDatabase()
    {
        var user = await SeedUserAsync();
        var farm = await _farmService.Handle(new CreateFarmCommand(user.Id, "Granja Test", "Lima"));
        var crop = await SeedCropAsync();
        var zone = await _zoneService.Handle(
            new CreateZoneCommand(farm.Id, crop.Id, -12.046, -77.042, "Semilla", null, null));

        var updated = await _zoneService.Handle(
            new UpdateZoneCommand(zone.Id, null, null, null, "Floración", DateTime.Today, null));

        Assert.Equal("Floración", updated.CurrentPhase);
        Assert.Equal(DateTime.Today, updated.PhaseStartDate);
    }

    [Fact]
    public async Task UpdateZone_NonExistentId_ThrowsKeyNotFoundException()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _zoneService.Handle(new UpdateZoneCommand(99999, null, null, null, "Floración", null, null)));
    }

    [Fact]
    public async Task UpdateZone_OnlyLatitudeSent_ThrowsArgumentException()
    {
        var user = await SeedUserAsync();
        var farm = await _farmService.Handle(new CreateFarmCommand(user.Id, "Granja Test", "Lima"));
        var crop = await SeedCropAsync();
        var zone = await _zoneService.Handle(
            new CreateZoneCommand(farm.Id, crop.Id, -12.046, -77.042, null, null, null));

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _zoneService.Handle(new UpdateZoneCommand(zone.Id, null, -13.0, null, null, null, null)));
    }

    public void Dispose() => _context.Dispose();
}