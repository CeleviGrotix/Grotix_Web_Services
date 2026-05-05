using MediatR;
using GrotixBackend.IAM.Application.Resources;

namespace GrotixBackend.IAM.Domain.Model.Commands
{
    public record LoginCommand(string Email, string Password): IRequest<LoginResponse>;
}