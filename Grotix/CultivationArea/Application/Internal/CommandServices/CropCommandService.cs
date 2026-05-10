using GrotixBackend.CultivationArea.Domain.Model.Aggregates;
using GrotixBackend.CultivationArea.Domain.Model.Commands;
using GrotixBackend.CultivationArea.Domain.Repositories;
using GrotixBackend.Shared.Domain.Repositories;

namespace GrotixBackend.CultivationArea.Application.Internal.CommandServices;

public class CropCommandService(
    ICropRepository cropRepository,
    IUnitOfWork unitOfWork
) : ICropCommandService
{
    public async Task<Crop> Handle(CreateCropCommand command)
    {
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
}
