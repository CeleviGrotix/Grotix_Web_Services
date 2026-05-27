using GrotixBackend.CultivationArea.Domain.Model.Aggregates;
using GrotixBackend.CultivationArea.Domain.Model.Commands;
using GrotixBackend.CultivationArea.Domain.Repositories;

namespace GrotixBackend.CultivationArea.Application.Internal.CommandServices;

public class CropCommandService(
    ICropRepository cropRepository,
    IZoneRepository zoneRepository,
    ICultivationAreaUnitOfWork unitOfWork
) : ICropCommandService
{
    public async Task<Crop> Handle(CreateCropCommand command)
    {
        if (await cropRepository.ExistsByCommonNameAsync(command.CommonName))
            throw new ArgumentException("Ya existe un cultivo con ese nombre común.");

        if (await cropRepository.ExistsByScientificNameAsync(command.ScientificName))
            throw new ArgumentException("Ya existe un cultivo con ese nombre científico.");

        var crop = new Crop(
            command.CommonName,
            command.ScientificName,
            command.OptimalTemperature,
            command.OptimalHumidity,
            command.OptimalLight,
            command.MaxStressTime,
            command.ImageUrl);

        await cropRepository.AddAsync(crop);
        await unitOfWork.CompleteAsync();
        return crop;
    }

    public async Task<Crop> Handle(UpdateCropCommand command)
    {
        var crop = await cropRepository.GetByIdAsync(command.CropId);
        if (crop == null)
            throw new KeyNotFoundException($"No existe el cultivo {command.CropId}.");

        if (await cropRepository.ExistsByCommonNameAsync(command.CommonName, excludingCropId: crop.Id))
            throw new ArgumentException("Ya existe un cultivo con ese nombre común.");

        if (await cropRepository.ExistsByScientificNameAsync(command.ScientificName, excludingCropId: crop.Id))
            throw new ArgumentException("Ya existe un cultivo con ese nombre científico.");

        crop.UpdateNames(command.CommonName, command.ScientificName);
        crop.UpdateBiologicalProfile(
            command.OptimalTemperature,
            command.OptimalHumidity,
            command.OptimalLight,
            command.MaxStressTime);
        crop.UpdateImageUrl(command.ImageUrl);

        await unitOfWork.CompleteAsync();
        return crop;
    }

    public async Task Handle(DeleteCropCommand command)
    {
        var crop = await cropRepository.GetByIdAsync(command.CropId);
        if (crop == null)
            throw new KeyNotFoundException($"No existe el cultivo {command.CropId}.");

        if (await zoneRepository.AnyByCropIdAsync(command.CropId))
            throw new InvalidOperationException(
                "No se puede eliminar el cultivo: hay zonas que lo referencian.");

        await cropRepository.DeleteAsync(crop);
        await unitOfWork.CompleteAsync();
    }
}
