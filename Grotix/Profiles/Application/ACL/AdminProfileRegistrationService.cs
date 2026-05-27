using GrotixBackend.Contracts.Auth.Roles;
using GrotixBackend.Contracts.Profiles.Provisioning;
using GrotixBackend.Profiles.Application.Internal.CommandServices;
using GrotixBackend.Profiles.Domain.Model.Commands;
using GrotixBackend.Profiles.Domain.Repositories;

namespace GrotixBackend.Profiles.Application.ACL;

public sealed class AdminProfileRegistrationService(
    IRoleRepository roleRepository,
    IAssociationRepository associationRepository,
    IUserCommandService userCommandService) : IAdminProfileRegistrationService
{
    public async Task<int> CreateUserAndReturnIdAsync(
        AdminCreateProfileUserRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!await roleRepository.ExistsAsync(request.RoleId))
            throw new ArgumentException($"El rol {request.RoleId} no existe.");

        var associationId = request.AssociationId is > 0 ? request.AssociationId : null;
        ValidateAssociationForRole(request.RoleId, associationId);

        if (associationId is { } assocId && !await associationRepository.ExistsAsync(assocId))
            throw new ArgumentException($"La asociación {assocId} no existe.");

        var user = await userCommandService.Handle(new CreateUserCommand(
            IdentityId: request.IdentityId,
            Email: request.Email,
            RoleId: request.RoleId,
            Name: request.Name,
            TaxId: request.TaxId,
            Phone: request.Phone,
            AssociationId: associationId,
            IsActive: request.IsActive));

        return user.Id;
    }

    private static void ValidateAssociationForRole(int roleId, int? associationId)
    {
        if (KnownRoleIds.IsSystemRole(roleId))
        {
            if (associationId.HasValue)
                throw new ArgumentException("Los roles sistema (admin, staff) no deben tener asociación.");

            return;
        }

        if (KnownRoleIds.IsAssociationUserRole(roleId))
        {
            if (!associationId.HasValue)
                throw new ArgumentException("Este rol de organización requiere AssociationId.");

            return;
        }

        throw new ArgumentException($"RoleId {roleId} no está soportado para alta administrativa.");
    }
}
