using MediatR;

namespace GrotixBackend.IAM.Domain.Model.Commands;

/// <summary>Comando del informe Profile (antes RegisterCommand).</summary>
public record CreateAccountCommand(string Email, string Password) : IRequest<int>;
