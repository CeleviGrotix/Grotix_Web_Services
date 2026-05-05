using MediatR;

namespace GrotixBackend.IAM.Domain.Model.Commands;

public record RegisterCommand(string Email, string Password) : IRequest<int>;