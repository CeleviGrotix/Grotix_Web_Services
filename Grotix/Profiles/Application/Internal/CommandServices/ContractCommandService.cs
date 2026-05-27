using GrotixBackend.Contracts.Auth.Lookup;
using GrotixBackend.Contracts.Profiles.Access;
using GrotixBackend.Profiles.Domain.Model.Aggregates;
using GrotixBackend.Profiles.Domain.Model.Commands;
using GrotixBackend.Profiles.Domain.Model.Enums;
using GrotixBackend.Profiles.Domain.Model.ValueObjects;
using GrotixBackend.Profiles.Domain.Repositories;

namespace GrotixBackend.Profiles.Application.Internal.CommandServices;

public class ContractCommandService(
    IContractRepository contractRepository,
    IAssociationRepository associationRepository,
    IIdentityLookupService identityLookupService,
    IUserRepository userRepository,
    IAssociationInviteRepository inviteRepository,
    IAssociationInviteCommandService inviteCommandService,
    IAssociationFarmOwnerSyncService associationFarmOwnerSyncService,
    IProfilesUnitOfWork unitOfWork
) : IContractCommandService
{
    public async Task<CreateContractResult> Handle(CreateContractCommand command)
    {
        if (!await associationRepository.ExistsAsync(command.AssociationId))
            throw new ArgumentException($"La asociación {command.AssociationId} no existe.");

        var adminEmailVo = UserEmail.Create(command.OrgAdminEmail.Trim());
        var adminEmail = adminEmailVo.Value;

        var existingOrgAdmin = await userRepository.GetUserAdminByAssociationIdAsync(command.AssociationId);
        if (existingOrgAdmin != null && !existingOrgAdmin.Email.Equals(adminEmailVo))
            throw new ArgumentException(
                "La asociación ya tiene un usuario administrador (user_admin) con otro correo.");

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

        if (existingOrgAdmin != null)
        {
            await unitOfWork.CompleteAsync();
            await associationFarmOwnerSyncService.SyncUnownedFarmsAsync(command.AssociationId);
            return new CreateContractResult(
                contract,
                adminEmail,
                AssignedOrgAdminUserId: existingOrgAdmin.Id,
                InviteSkipped: true);
        }

        var pendingInvite = await inviteRepository.GetPendingInviteForEmailAsync(command.AssociationId, adminEmail);
        if (pendingInvite != null)
        {
            if (pendingInvite.RoleId != (int)RoleType.user_admin)
                throw new ArgumentException(
                    "Ya existe una invitación pendiente para este correo con un rol distinto a user_admin.");

            await unitOfWork.CompleteAsync();
            return new CreateContractResult(
                contract,
                adminEmail,
                OrgAdminInviteId: pendingInvite.Id,
                InviteSkipped: true);
        }

        if (await identityLookupService.ExistsByEmailAsync(adminEmail))
        {
            var orgAdmin = await userRepository.GetByEmailAsync(adminEmail)
                           ?? await TryLoadUserByIdentityEmailAsync(adminEmail);

            if (orgAdmin == null)
                throw new ArgumentException(
                    "El correo está registrado en identidad pero no tiene perfil en Profiles.");

            orgAdmin.AssignRole((int)RoleType.user_admin);
            orgAdmin.AssignAssociation(command.AssociationId);

            await unitOfWork.CompleteAsync();
            await associationFarmOwnerSyncService.SyncUnownedFarmsAsync(command.AssociationId);
            return new CreateContractResult(
                contract,
                adminEmail,
                AssignedOrgAdminUserId: orgAdmin.Id,
                InviteSkipped: true);
        }

        var inviteResult = await inviteCommandService.Handle(new CreateAssociationInviteCommand(
            command.AssociationId,
            adminEmail,
            (int)RoleType.user_admin,
            ExpiresAt: null,
            CreatedByUserId: null));

        await unitOfWork.CompleteAsync();
        return new CreateContractResult(
            contract,
            adminEmail,
            OrgAdminInviteId: inviteResult.InviteId,
            OrgAdminInvitePlaintextToken: inviteResult.PlaintextToken);
    }

    public async Task<Contract?> Handle(UpdateContractCommand command)
    {
        var contract = await contractRepository.GetByIdAsync(command.ContractId);
        if (contract == null)
            throw new ArgumentException("El contrato especificado no existe.");

        contract.Update(
            command.EndDate,
            command.Status,
            command.MaxZones,
            command.MaxMicrocontrollers,
            command.IsSuspended,
            command.TotalAmount,
            command.Currency,
            command.PaymentFrequency);

        await unitOfWork.CompleteAsync();
        return contract;
    }

    public async Task Handle(DeleteContractCommand command)
    {
        var contract = await contractRepository.GetByIdAsync(command.ContractId);
        if (contract == null)
            throw new ArgumentException("El contrato especificado no existe.");

        contract.Update(
            null,
            ContractStatus.Cancelled,
            null,
            null,
            true,
            null,
            null,
            null);

        await unitOfWork.CompleteAsync();
    }

    private async Task<User?> TryLoadUserByIdentityEmailAsync(string adminEmail)
    {
        var identityId = await identityLookupService.GetIdentityIdByEmailAsync(adminEmail);
        return identityId == null ? null : await userRepository.GetByIdentityIdAsync(identityId.Value);
    }
}
