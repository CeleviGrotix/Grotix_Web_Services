using GrotixBackend.HardwareDevice.Domain.Model.Aggregates;

namespace GrotixBackend.HardwareDevice.Domain.Repositories;

public interface IMicrocontrollerRepository
{
    Task<Microcontroller?> GetByIdAsync(int id);
    Task<Microcontroller?> GetByMacAddressAsync(string macAddress);
    Task<IReadOnlyList<Microcontroller>> ListAsync(string? status, int? zoneId);
    Task<IReadOnlyList<Microcontroller>> ListByZoneAsync(int zoneId);
    Task<IReadOnlyList<Microcontroller>> ListOnlineStaleAsync(DateTime lastSeenBefore, CancellationToken cancellationToken = default);
    Task AddAsync(Microcontroller device);
    Task DeleteAsync(Microcontroller device);
}
