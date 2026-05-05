using MediatR;
namespace GrotixBackend.IAM.Domain.Model.Commands
{
    public record RegisterCommand(int UserId, string Email, string Password): IRequest<int>;
}