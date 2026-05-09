using GrotixBackend.IAM.Domain.Model.Aggregates;
using GrotixBackend.IAM.Domain.Model.ValueObjects;
using GrotixBackend.IAM.Domain.Repositories;
using GrotixBackend.Profiles.Domain.Model.Aggregates;
using GrotixBackend.Profiles.Domain.Model.Commands;
using GrotixBackend.Profiles.Domain.Model.ValueObjects;
using GrotixBackend.Profiles.Domain.Repositories;
using GrotixBackend.Profiles.Domain.Services;
using GrotixBackend.Shared.Domain.Repositories;

namespace GrotixBackend.Profiles.Application.Internal.CommandServices;

public class ContractCommandService(
    IContractRepository contractRepository,
    IAssociationRepository associationRepository,
    IIdentityRepository identityRepository,
    IUserRepository userRepository,
    IUserCommandService userCommandService,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork
) : IContractCommandService
{
    public async Task<CreateContractResult> Handle(CreateContractCommand command)
    {
        if (!await associationRepository.ExistsAsync(command.AssociationId))
            throw new ArgumentException($"La asociación {command.AssociationId} no existe.");

        if (await userRepository.HasUserAdminForAssociationAsync(command.AssociationId))
            throw new ArgumentException(
                "La asociación ya tiene un usuario administrador (user_admin). No se puede crear otro desde este flujo.");

        var adminEmail = command.OrgAdminEmail.Trim();
        if (await identityRepository.ExistsByEmailAsync(adminEmail))
            throw new ArgumentException("El correo del administrador ya está registrado.");

        Identity.VerifyPasswordStrength(command.OrgAdminPassword);

        var hash = passwordHasher.Hash(command.OrgAdminPassword);
        var identity = new Identity(adminEmail, PasswordHash.FromHash(hash));

        await identityRepository.AddAsync(identity);
        await unitOfWork.CompleteAsync();

        var adminUser = await userCommandService.Handle(new CreateUserCommand(
            identity.Id,
            adminEmail,
            RoleId: (int)RoleType.user_admin,
            Name: command.OrgAdminName,
            AssociationId: command.AssociationId));

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

        return new CreateContractResult(contract, adminUser.Id);
    }
}
