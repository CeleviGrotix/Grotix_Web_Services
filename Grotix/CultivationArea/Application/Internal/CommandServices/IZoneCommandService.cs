using GrotixBackend.CultivationArea.Domain.Model.Aggregates;
using GrotixBackend.CultivationArea.Domain.Model.Commands;

namespace GrotixBackend.CultivationArea.Application.Internal.CommandServices;

public interface IZoneCommandService
{
    Task<Zone> Handle(CreateZoneCommand command);
    Task<Zone> Handle(UpdateZoneCommand command);
}
