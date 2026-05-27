using GrotixBackend.Profiles.Domain.Model.Aggregates;
using GrotixBackend.Profiles.Domain.Repositories;
using GrotixBackend.Shared.Infrastructure.Persistence.EFC.Configuration;
using GrotixBackend.Shared.Infrastructure.Persistence.EFC.Repositories;

namespace GrotixBackend.Profiles.Infrastructure.Repositories;

public class CoreDbAssociationRepository(ProfilesDbContext context)
    : BaseRepository<Association>(context), IAssociationRepository;
