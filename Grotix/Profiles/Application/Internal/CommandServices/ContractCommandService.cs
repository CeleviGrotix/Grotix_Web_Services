using GrotixBackend.IAM.Domain.Repositories;
using GrotixBackend.Profiles.Domain.Model.Aggregates;
using GrotixBackend.Profiles.Domain.Model.Commands;
using GrotixBackend.Profiles.Domain.Model.ValueObjects;
using GrotixBackend.Profiles.Domain.Repositories;
using GrotixBackend.Shared.Domain.Repositories;

namespace GrotixBackend.Profiles.Application.Internal.CommandServices;

public class ContractCommandService(
    IContractRepository contractRepository,
    IAssociationRepository associationRepository,
    IIdentityRepository identityRepository,
    IUserRepository userRepository,
    IAssociationInviteRepository inviteRepository,
    IAssociationInviteCommandService inviteCommandService,
    IUnitOfWork unitOfWork
) : IContractCommandService
{
    public async Task<CreateContractResult> Handle(CreateContractCommand command)
    {
        if (!await associationRepository.ExistsAsync(command.AssociationId))
            throw new ArgumentException($"La asociación {command.AssociationId} no existe.");

        if (await userRepository.HasUserAdminForAssociationAsync(command.AssociationId))
            throw new ArgumentException(
                "La asociación ya tiene un usuario administrador (user_admin). Use una invitación si debe registrarse otro contacto.");

        var adminEmailVo = UserEmail.Create(command.OrgAdminEmail.Trim());
        var adminEmail = adminEmailVo.Value;

        if (await identityRepository.ExistsByEmailAsync(adminEmail))
            throw new ArgumentException("El correo del administrador ya está registrado.");

        if (await inviteRepository.HasPendingInviteForEmailAsync(command.AssociationId, adminEmail))
            throw new ArgumentException(
                "Ya existe una invitación pendiente para este correo en esta asociación.");

        var contract = new Contract(
            command.AssociationId,
            command.StartDate,
            command.EndDate,
            command.Status,
            command.MaxZones,
            command.MaxMicrocontrollers,
            command.TotalAmount,
            command.Currency,
            command.PaymentFrequency,
            command.IsSuspended);

        await contractRepository.AddAsync(contract);
        await unitOfWork.CompleteAsync();

        var inviteResult = await inviteCommandService.Handle(new CreateAssociationInviteCommand(
            command.AssociationId,
            adminEmail,
            (int)RoleType.user_admin,
            ExpiresAt: null,
            CreatedByUserId: null));

        return new CreateContractResult(
            contract,
            inviteResult.InviteId,
            inviteResult.PlaintextToken,
            adminEmail);
    }
}
