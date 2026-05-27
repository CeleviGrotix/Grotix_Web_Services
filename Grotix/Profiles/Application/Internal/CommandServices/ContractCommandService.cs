using GrotixBackend.Contracts.Auth.Lookup;
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
    IProfilesUnitOfWork unitOfWork
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

        if (await identityLookupService.ExistsByEmailAsync(adminEmail))
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

public async Task<Contract?> Handle(UpdateContractCommand command)
    {
        // 1. Buscamos el contrato usando el nombre correcto: GetByIdAsync
        var contract = await contractRepository.GetByIdAsync(command.ContractId);
        if (contract == null)
            throw new ArgumentException("El contrato especificado no existe.");

        // 2. Aplicamos los cambios
        contract.Update(
            command.EndDate, 
            command.Status, 
            command.MaxZones, 
            command.MaxMicrocontrollers, 
            command.IsSuspended,
            command.TotalAmount, // <--- Pasar aquí
            command.PaymentFrequency
            );

        // 3. Guardamos. No hace falta 'Update()' porque Entity Framework 
        // rastrea los cambios automáticamente. Solo confirmamos la transacción.
        await unitOfWork.CompleteAsync();

        return contract;
    }

public async Task Handle(DeleteContractCommand command)
{
    var contract = await contractRepository.GetByIdAsync(command.ContractId);
    if (contract == null)
        throw new ArgumentException("El contrato especificado no existe.");

    // Usamos el valor directo del Enum. 
    // Si tu Enum no tiene 'Canceled', puedes usar 'Draft' u otro estado de baja.
    contract.Update(
        null, 
        ContractStatus.Cancelled, 
        null, 
        null, 
        true,   // IsSuspended = true
        null, 
        null);

    await unitOfWork.CompleteAsync();
}
}
