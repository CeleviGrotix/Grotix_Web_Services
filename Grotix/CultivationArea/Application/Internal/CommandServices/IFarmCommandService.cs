using GrotixBackend.CultivationArea.Domain.Model.Aggregates;
using GrotixBackend.CultivationArea.Domain.Model.Commands;

namespace GrotixBackend.CultivationArea.Application.Internal.CommandServices;

public interface IFarmCommandService
{
    Task<Farm> Handle(CreateFarmCommand command);
    Task<Farm> Handle(UpdateFarmCommand command);
}
