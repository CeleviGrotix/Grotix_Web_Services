using GrotixBackend.Profiles.Application.Internal.QueryServices;
using GrotixBackend.Profiles.Domain.Model.Aggregates;
using GrotixBackend.Profiles.Domain.Model.Queries;
using GrotixBackend.Profiles.Domain.Repositories;

namespace GrotixBackend.Profiles.Application.Internal.QueryServices;

public class UserQueryService(IUserRepository userRepository) : IUserQueryService
{
    public async Task<User?> Handle(GetUserByIdQuery query)
    {
        return await userRepository.GetByIdAsync(query.UserId);
    }

    public async Task<User?> Handle(GetUserByIdentityQuery query)
    {
        return await userRepository.GetByIdentityIdAsync(query.IdentityId);
    }

    public Task<IReadOnlyList<User>> Handle(GetAllFarmersQuery query) =>
        userRepository.ListFarmersOrderedByIdAsync();

    public Task<IReadOnlyList<User>> Handle(GetFarmersByAssociationQuery query) =>
        userRepository.ListFarmersByAssociationIdAsync(query.AssociationId);

    public Task<User?> Handle(GetFarmerByIdQuery query) =>
        userRepository.GetFarmerByIdAsync(query.UserId);
}