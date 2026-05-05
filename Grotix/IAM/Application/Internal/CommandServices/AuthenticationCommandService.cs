using GrotixBackend.IAM.Application.Internal.OutboundServices;
using GrotixBackend.IAM.Application.Internal.OutboundServices.ACL;
using GrotixBackend.IAM.Application.Resources;
using GrotixBackend.IAM.Domain.Model.Aggregates;
using GrotixBackend.IAM.Domain.Model.Commands;
using GrotixBackend.IAM.Domain.Model.Services;
using GrotixBackend.IAM.Domain.Model.ValueObjects;
using GrotixBackend.IAM.Domain.Repositories;
using GrotixBackend.Shared.Domain.Repositories;

namespace GrotixBackend.IAM.Application.Internal.CommandServices;

public class AuthenticationCommandService(
    IIdentityRepository identityRepository,
    IUnitOfWork unitOfWork,
    IExternalProfileService profileService, // Nuestra ACL para Profiles
    ITokenService tokenService // Probablemente necesario para el Login
) : IIdentityCommandService
{
    // 1. Manejo del Registro (Retorna int según tu interfaz)
    public async Task<int> Handle(RegisterCommand command)
    {
        // 1. Validar fortaleza de la contraseña
        Identity.VerifyPasswordStrength(command.Password);

        // 2. Si pasa la validación, procedemos
        var passwordHash = new PasswordHash(command.Password);
        var identity = new Identity(0, command.Email, passwordHash.HashedValue);

        await identityRepository.AddAsync(identity);
        await unitOfWork.CompleteAsync();

        await profileService.CreateUserAndReturnId(identity.Id, identity.UserName);

        return identity.Id;
    }   

    // 2. Manejo del Login (Retorna LoginResponse según tu interfaz)
    public async Task<LoginResponse> Handle(LoginCommand command)
    {
        // Aquí va tu lógica existente de búsqueda de usuario y validación de hash
        // Luego generas el token usando el tokenService
        // Por ahora lo dejo como marcador si ya tienes esta lógica implementada
        throw new NotImplementedException("Implementar validación de credenciales y generación de token aquí");
    }
}