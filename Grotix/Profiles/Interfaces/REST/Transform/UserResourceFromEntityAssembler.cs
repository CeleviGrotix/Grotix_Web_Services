using GrotixBackend.Profiles.Domain.Model.Aggregates;
using GrotixBackend.Profiles.Interfaces.REST.Resources;

namespace GrotixBackend.Profiles.Interfaces.REST.Transform;

public static class UserResourceFromEntityAssembler
{
    public static UserResource ToResourceFromEntity(User entity)
    {
        return new UserResource(
            entity.Id,
            entity.Name,
            entity.Email,
            entity.TaxId,
            entity.ProfilePicture
        );
    }
}