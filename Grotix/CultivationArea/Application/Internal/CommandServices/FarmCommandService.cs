using GrotixBackend.CultivationArea.Domain.Model.Aggregates;
using GrotixBackend.CultivationArea.Domain.Model.Commands;
using GrotixBackend.CultivationArea.Domain.Repositories;
using GrotixBackend.Shared.Domain.Repositories;

namespace GrotixBackend.CultivationArea.Application.Internal.CommandServices;

public class FarmCommandService(
    IFarmRepository farmRepository,
    IUnitOfWork unitOfWork
) : IFarmCommandService
{
    public async Task<Farm> Handle(CreateFarmCommand command)
    {
        var farm = new Farm(command.UserId, command.Name, command.Location);
        await farmRepository.AddAsync(farm);
        await unitOfWork.CompleteAsync();
        return farm;
    }

    public async Task<Farm> Handle(UpdateFarmCommand command)
    {
        var farm = await farmRepository.GetByIdAsync(command.FarmId)
                   ?? throw new KeyNotFoundException($"Granja {command.FarmId} no encontrada.");
        farm.Update(command.Name, command.Location);
        await unitOfWork.CompleteAsync();
        return farm;
    }
}
