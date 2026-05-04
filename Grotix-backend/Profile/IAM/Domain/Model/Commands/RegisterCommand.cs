using MediatR;
namespace GrotixBackend.IAM.Domain.Model.Commands
{
    public record RegisterCommand(int UserId, string Username, string Password): IRequest<int>;
}