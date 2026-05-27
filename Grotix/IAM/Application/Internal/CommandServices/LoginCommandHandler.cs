using MediatR;
using GrotixBackend.Contracts.Auth.Lookup;
using GrotixBackend.IAM.Application.Internal.OutboundServices;
using GrotixBackend.IAM.Application.Resources;
using GrotixBackend.IAM.Domain.Model.Commands;
using GrotixBackend.IAM.Domain.Repositories;
using GrotixBackend.Contracts.Auth.Security;

namespace GrotixBackend.IAM.Application.Internal.CommandServices;

public class LoginCommandHandler(
    IIdentityRepository identityRepository,
    IUserAuthorizationContextService authorizationContextService,
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
        var authorizationContext = await authorizationContextService.GetByIdentityIdAsync(identity.Id, cancellationToken);
        if (authorizationContext != null && !authorizationContext.IsActive)
            return new LoginResponse(0, command.Email, false, "La cuenta está desactivada.");

        if (authorizationContext != null)
        {
            roleNames = authorizationContext.RoleNames.ToList();
            permissionCodes = authorizationContext.PermissionCodes;
        }

        var token = tokenService.GenerateToken(identity, roleNames, permissionCodes);

        return new LoginResponse(identity.Id, identity.UserName, true, "Login exitoso.", token);
    }
}