using GrotixBackend.Contracts.Profiles.Access;
using GrotixBackend.CultivationArea.Domain.Model.Aggregates;
using GrotixBackend.CultivationArea.Domain.Model.Commands;
using GrotixBackend.CultivationArea.Domain.Repositories;

namespace GrotixBackend.CultivationArea.Application.Internal.CommandServices;

public class FarmCommandService(
    IFarmRepository farmRepository,
    IAssociationExistenceService associationExistenceService,
    IAssociationOwnerLookupService associationOwnerLookupService,
    ICultivationAreaUnitOfWork unitOfWork
) : IFarmCommandService
{
    public async Task<Farm> Handle(CreateFarmCommand command)
    {
        if (!await associationExistenceService.ExistsAsync(command.AssociationId))
            throw new ArgumentException($"La asociación {command.AssociationId} no existe.");

        var ownerUserId = await associationOwnerLookupService.GetOwnerUserIdAsync(command.AssociationId);
        if (await farmRepository.ExistsByAssociationAndNameAsync(command.AssociationId, command.Name))
            throw new ArgumentException("Ya existe una granja con ese nombre en la organización.");

        var farm = new Farm(ownerUserId, command.AssociationId, command.Name, command.Location);
        await farmRepository.AddAsync(farm);
        await unitOfWork.CompleteAsync();
        return farm;
    }

    public async Task<Farm> Handle(UpdateFarmCommand command)
    {
        var farm = await farmRepository.GetByIdAsync(command.FarmId)
                   ?? throw new KeyNotFoundException($"Granja {command.FarmId} no encontrada.");

        if (await farmRepository.ExistsByAssociationAndNameAsync(
                farm.AssociationId,
                command.Name,
                excludingFarmId: farm.Id))
            throw new ArgumentException("Ya existe una granja con ese nombre en la organización.");

        farm.Update(command.Name, command.Location);
        await unitOfWork.CompleteAsync();
        return farm;
    }
}
