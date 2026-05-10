using GrotixBackend.Profiles.Domain.Model.Aggregates;
using GrotixBackend.Profiles.Domain.Model.Commands;

namespace GrotixBackend.Profiles.Application.Internal.CommandServices;

public interface IContractCommandService
{
    Task<CreateContractResult> Handle(CreateContractCommand command);
    Task<Contract?> Handle(UpdateContractCommand command);
    Task Handle(DeleteContractCommand command);
}
