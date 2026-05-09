using MediatR;

namespace GrotixBackend.IAM.Domain.Model.Commands;

/// <summary>Comando del informe Profile (antes RegisterCommand).</summary>
/// <param name="InviteToken">Token opaco emitido al crear una invitación (union a organización).</param>
public record CreateAccountCommand(string Email, string Password, string InviteToken) : IRequest<int>;
