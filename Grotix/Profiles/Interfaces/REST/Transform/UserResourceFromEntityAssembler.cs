// Profiles/Interfaces/REST/Transform/UserResourceFromEntityAssembler.cs
using GrotixBackend.Profiles.Domain.Model.Aggregates;
using GrotixBackend.Profiles.Interfaces.REST.Resources;

namespace GrotixBackend.Profiles.Interfaces.REST.Transform;

public static class UserResourceFromEntityAssembler
{
    public static UserResource ToResourceFromEntity(User entity)
    {
        var prefs = entity.GetPreferences();
        return new UserResource(
            entity.Id,
            entity.IdentityId,
            entity.Name,
            entity.Email.Value,
            entity.TaxId,
            entity.Phone,
            entity.RoleId,
            entity.AssociationId,
            entity.ProfilePicture,
            new UserPreferencesResource(prefs.Push, prefs.Email),
            entity.CreatedAt,
            entity.UpdatedAt
        );
    }
}