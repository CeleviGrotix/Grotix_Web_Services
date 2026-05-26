using GrotixBackend.Contracts.Auth.Admin;
using GrotixBackend.IAM.Domain.Model.Commands;
using MediatR;

namespace GrotixBackend.IAM.Application.ACL;

public sealed class AdminIdentityRegistrationService(IMediator mediator) : IAdminIdentityRegistrationService
{
    public async Task<AdminRegisterUserResponse> RegisterAsync(
        AdminRegisterUserRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new AdminRegisterUserCommand(
            request.Email,
            request.Password,
            request.RoleId,
            request.AssociationId,
            request.Name,
            request.TaxId,
            request.Phone,
            request.IsActive), cancellationToken);

        return new AdminRegisterUserResponse(result.IdentityId, result.UserId);
    }
}
