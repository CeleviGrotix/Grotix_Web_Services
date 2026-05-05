using MediatR;
using GrotixBackend.IAM.Application.Internal.OutboundServices;
using GrotixBackend.IAM.Application.Resources;
using GrotixBackend.IAM.Domain.Model.Commands;
using GrotixBackend.IAM.Domain.Repositories;
using GrotixBackend.Shared.Domain.Repositories;

namespace GrotixBackend.IAM.Application.Internal.CommandServices;

public class LoginCommandHandler(
    IIdentityRepository identityRepository,
    ITokenService tokenService
) : IRequestHandler<LoginCommand, LoginResponse>
{
    public async Task<LoginResponse> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        var identity = await identityRepository.GetByEmailAsync(command.Email);

        if (identity == null || !identity.VerifyPassword(command.Password))
            return new LoginResponse(0, command.Email, false, "Credenciales inválidas.");

        var token = tokenService.GenerateToken(identity, ["User"]);

        return new LoginResponse(identity.Id, identity.UserName, true, "Login exitoso.", token);
    }
}