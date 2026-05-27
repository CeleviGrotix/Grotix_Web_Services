using GrotixBackend.Contracts.Auth.Admin;
using GrotixBackend.Contracts.Auth.Identity;
using GrotixBackend.Contracts.Auth.Notifications;
using GrotixBackend.Contracts.Auth.Security;
using GrotixBackend.Contracts.Profiles.Provisioning;
using GrotixBackend.IAM.Domain.Model.Aggregates;
using GrotixBackend.IAM.Domain.Model.ValueObjects;
using GrotixBackend.IAM.Domain.Repositories;
using GrotixBackend.Shared.Domain.Repositories;
using MediatR;

namespace GrotixBackend.IAM.Application.ACL;

public sealed class AdminIdentityRegistrationService(
    IIdentityRepository identityRepository,
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    IAdminProfileRegistrationService adminProfileRegistrationService,
    IMediator mediator) : IAdminIdentityRegistrationService
{
    public async Task<AdminRegisterUserResponse> RegisterAsync(
        AdminRegisterUserRequest request,
        CancellationToken cancellationToken = default)
    {
        if (await identityRepository.ExistsByEmailAsync(request.Email))
            throw new ApplicationException($"El correo '{request.Email}' ya está registrado.");

        Identity.VerifyPasswordStrength(request.Password);
        var normalizedEmail = AuthEmail.Normalize(request.Email);
        var hash = passwordHasher.Hash(request.Password);
        var identity = new Identity(normalizedEmail, PasswordHash.FromHash(hash));

        await identityRepository.AddAsync(identity);
        await unitOfWork.CompleteAsync();

        var userId = await adminProfileRegistrationService.CreateUserAndReturnIdAsync(
            new AdminCreateProfileUserRequest(
                identity.Id,
                normalizedEmail,
                request.RoleId,
                request.AssociationId,
                request.Name,
                request.TaxId,
                request.Phone,
                request.IsActive),
            cancellationToken);

        await mediator.Publish(new UserRegisteredNotification(identity.Id, identity.UserName), cancellationToken);

        return new AdminRegisterUserResponse(identity.Id, userId);
    }
}
