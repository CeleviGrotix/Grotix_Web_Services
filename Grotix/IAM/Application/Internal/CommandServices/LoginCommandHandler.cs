using MediatR;
using GrotixBackend.IAM.Application.Internal.OutboundServices;
using GrotixBackend.IAM.Application.Resources;
using GrotixBackend.IAM.Domain.Model.Commands;
using GrotixBackend.IAM.Domain.Repositories;
using GrotixBackend.Profiles.Domain.Repositories;
using GrotixBackend.Profiles.Domain.Services;

namespace GrotixBackend.IAM.Application.Internal.CommandServices;

public class LoginCommandHandler(
    IIdentityRepository identityRepository,
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    ITokenService tokenService,
    IPasswordHasher passwordHasher
) : IRequestHandler<LoginCommand, LoginResponse>
{
    public async Task<LoginResponse> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        var identity = await identityRepository.GetByEmailAsync(command.Email);

        if (identity == null ||
            !passwordHasher.Verify(command.Password, identity.HashedPassword.HashedValue))
            return new LoginResponse(0, command.Email, false, "Credenciales inválidas.");

        var roleNames = new List<string>();
        IReadOnlyList<string> permissionCodes = Array.Empty<string>();
        var profile = await userRepository.GetByIdentityIdAsync(identity.Id);
        if (profile != null && !profile.IsActive)
            return new LoginResponse(0, command.Email, false, "La cuenta está desactivada.");

        if (profile != null)
        {
            var roleEntity = await roleRepository.GetByIdAsync(profile.RoleId);
            if (roleEntity != null)
                roleNames = new List<string> { roleEntity.Name };
            permissionCodes = await roleRepository.GetPermissionCodesByRoleIdAsync(profile.RoleId);
        }

        var token = tokenService.GenerateToken(identity, roleNames, permissionCodes);

        return new LoginResponse(identity.Id, identity.UserName, true, "Login exitoso.", token);
    }
}