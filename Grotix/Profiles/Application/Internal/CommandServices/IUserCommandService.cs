using GrotixBackend.Profiles.Domain.Model.Aggregates;
using GrotixBackend.Profiles.Domain.Model.Commands;

namespace GrotixBackend.Profiles.Application.Internal.CommandServices;

public interface IUserCommandService
{
    Task<User?> Handle(CreateUserCommand command);


}