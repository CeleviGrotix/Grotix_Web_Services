using GrotixBackend.CultivationArea.Domain.Model.Aggregates;
using GrotixBackend.CultivationArea.Domain.Model.Commands;
using GrotixBackend.CultivationArea.Domain.Repositories;

namespace GrotixBackend.CultivationArea.Application.Internal.CommandServices;

public class ZoneCommandService(
    IZoneRepository zoneRepository,
    ICropRepository cropRepository,
    ICultivationAreaUnitOfWork unitOfWork
) : IZoneCommandService
{
    public async Task<Zone> Handle(CreateZoneCommand command)
    {
        if (!await cropRepository.ExistsAsync(command.CropId))
            throw new ArgumentException($"El cultivo {command.CropId} no existe.");

        var zone = new Zone(
            command.FarmId,
            command.CropId,
            command.Name,
            command.Latitude,
            command.Longitude,
            command.CurrentPhase,
            command.PhaseStartDate,
            command.ImageUrl);

        await zoneRepository.AddAsync(zone);
        await unitOfWork.CompleteAsync();
        return zone;
    }

    public async Task<Zone> Handle(UpdateZoneCommand command)
    {
        var zone = await zoneRepository.GetByIdAsync(command.ZoneId)
                   ?? throw new KeyNotFoundException($"Zona {command.ZoneId} no encontrada.");

        if (command.CropId.HasValue)
        {
            if (!await cropRepository.ExistsAsync(command.CropId.Value))
                throw new ArgumentException($"El cultivo {command.CropId} no existe.");
            zone.ReassignCrop(command.CropId.Value);
        }

        if (command.Latitude.HasValue != command.Longitude.HasValue)
            throw new ArgumentException("Latitude y Longitude deben enviarse juntos.");

        if (command.Latitude.HasValue && command.Longitude.HasValue)
            zone.UpdateCoordinates(command.Latitude.Value, command.Longitude.Value);

        if (command.Name != null)
            zone.UpdateName(command.Name);

        if (command.CurrentPhase != null || command.PhaseStartDate.HasValue)
            zone.UpdatePhase(command.CurrentPhase ?? zone.CurrentPhase, command.PhaseStartDate ?? zone.PhaseStartDate);

        if (command.ImageUrl is not null)
            zone.UpdateImageUrl(string.IsNullOrWhiteSpace(command.ImageUrl) ? null : command.ImageUrl);

        await unitOfWork.CompleteAsync();
        return zone;
    }
}
