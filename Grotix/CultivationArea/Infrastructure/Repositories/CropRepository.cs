using GrotixBackend.CultivationArea.Domain.Model.Aggregates;
using GrotixBackend.CultivationArea.Domain.Repositories;
using GrotixBackend.Shared.Infrastructure.Persistence.EFC.Configuration;
using GrotixBackend.Shared.Infrastructure.Persistence.EFC.Repositories;

namespace GrotixBackend.CultivationArea.Infrastructure.Repositories;

public class CropRepository(AppDbContext context)
    : BaseRepository<Crop>(context), ICropRepository;
