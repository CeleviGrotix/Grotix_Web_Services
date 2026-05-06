using GrotixBackend.Profiles.Domain.Model.Aggregates;
using GrotixBackend.Profiles.Domain.Model.Commands;

namespace GrotixBackend.Profiles.Application.Internal.CommandServices;

public interface IContractCommandService
{
    Task<Contract> Handle(CreateContractCommand command);
}
