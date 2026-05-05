using GrotixBackend.IAM.Domain.Model.Commands;
using GrotixBackend.IAM.Application.Resources;

namespace GrotixBackend.IAM.Domain.Model.Services;

public interface IIdentityCommandService
{
    Task<int> Handle(RegisterCommand command);

    Task<LoginResponse> Handle(LoginCommand command);
}