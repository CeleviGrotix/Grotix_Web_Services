using GrotixBackend.CultivationArea.Domain.Model.Aggregates;
using GrotixBackend.Shared.Domain.Repositories;

namespace GrotixBackend.CultivationArea.Domain.Repositories;

public interface ICropRepository : IAsyncRepository<Crop>
{
}
