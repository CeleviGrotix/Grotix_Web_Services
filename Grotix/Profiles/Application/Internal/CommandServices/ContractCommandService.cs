using GrotixBackend.Profiles.Domain.Model.Aggregates;
using GrotixBackend.Profiles.Domain.Model.Commands;
using GrotixBackend.Profiles.Domain.Repositories;
using GrotixBackend.Shared.Domain.Repositories;

namespace GrotixBackend.Profiles.Application.Internal.CommandServices;

public class ContractCommandService(
    IContractRepository contractRepository,
    IAssociationRepository associationRepository,
    IUnitOfWork unitOfWork
) : IContractCommandService
{
    public async Task<Contract> Handle(CreateContractCommand command)
    {
        if (!await associationRepository.ExistsAsync(command.AssociationId))
            throw new ArgumentException($"La asociación {command.AssociationId} no existe.");

        var contract = new Contract(
            command.AssociationId,
            command.StartDate,
            command.EndDate,
            command.Status,
            command.MaxZones,
            command.MaxMicrocontrollers,
            command.TotalAmount,
            command.Currency,
            command.PaymentFrequency,
            command.IsSuspended);

        await contractRepository.AddAsync(contract);
        await unitOfWork.CompleteAsync();
        return contract;
    }
}
