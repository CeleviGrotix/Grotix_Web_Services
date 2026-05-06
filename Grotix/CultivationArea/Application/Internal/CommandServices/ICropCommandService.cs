using GrotixBackend.CultivationArea.Domain.Model.Aggregates;
using GrotixBackend.CultivationArea.Domain.Model.Commands;

namespace GrotixBackend.CultivationArea.Application.Internal.CommandServices;

public interface ICropCommandService
{
    Task<Crop> Handle(CreateCropCommand command);
}
