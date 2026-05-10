using MediatR;
using GrotixBackend.IAM.Domain.Model.Aggregates;
using GrotixBackend.IAM.Domain.Model.Commands;
using GrotixBackend.IAM.Domain.Model.ValueObjects;
using GrotixBackend.IAM.Domain.Repositories;
using GrotixBackend.Profiles.Application.Internal.CommandServices;
using GrotixBackend.Profiles.Domain.Model.Commands;
using GrotixBackend.Profiles.Domain.Model.Notifications;
using GrotixBackend.Profiles.Domain.Model.ValueObjects;
using GrotixBackend.Profiles.Domain.Repositories;
using GrotixBackend.Profiles.Domain.Services;
using GrotixBackend.Shared.Domain.Repositories;

namespace GrotixBackend.IAM.Application.Internal.CommandServices;

public class AdminRegisterUserHandler(
    IIdentityRepository identityRepository,
    IRoleRepository roleRepository,
    IAssociationRepository associationRepository,
    IUserCommandService userCommandService,
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    IMediator mediator
) : IRequestHandler<AdminRegisterUserCommand, AdminRegisterUserResult>
{
    public async Task<AdminRegisterUserResult> Handle(AdminRegisterUserCommand command, CancellationToken cancellationToken)
    {
        if (await identityRepository.ExistsByEmailAsync(command.Email))
            throw new ApplicationException($"El correo '{command.Email}' ya está registrado.");

        if (!await roleRepository.ExistsAsync(command.RoleId))
            throw new ArgumentException($"El rol {command.RoleId} no existe.");

        ValidateAssociationForRole(command.RoleId, command.AssociationId);

        if (command.AssociationId is { } assocId && !await associationRepository.ExistsAsync(assocId))
            throw new ArgumentException($"La asociación {assocId} no existe.");

        Identity.VerifyPasswordStrength(command.Password);
        var hash = passwordHasher.Hash(command.Password);
        var identity = new Identity(command.Email, PasswordHash.FromHash(hash));

        await identityRepository.AddAsync(identity);
        await unitOfWork.CompleteAsync();

        var user = await userCommandService.Handle(new CreateUserCommand(
            IdentityId: identity.Id,
            Email: command.Email,
            RoleId: command.RoleId,
            Name: command.Name,
            TaxId: command.TaxId,
            Phone: command.Phone,
            AssociationId: command.AssociationId,
            IsActive: command.IsActive));

        await mediator.Publish(new UserRegisteredNotification(identity.Id, identity.UserName), cancellationToken);

        return new AdminRegisterUserResult(identity.Id, user.Id);
    }

    private static void ValidateAssociationForRole(int roleId, int? associationId)
    {
        if (roleId is (int)RoleType.admin or (int)RoleType.staff)
        {
            if (associationId.HasValue)
                throw new ArgumentException("Los roles sistema (admin, staff) no deben tener asociación.");
            return;
        }

        if (roleId is (int)RoleType.user_admin or (int)RoleType.user_basic or (int)RoleType.user_advanced)
        {
            if (!associationId.HasValue)
                throw new ArgumentException("Este rol de organización requiere AssociationId.");
            return;
        }

        throw new ArgumentException($"RoleId {roleId} no está soportado para alta administrativa.");
    }
}
