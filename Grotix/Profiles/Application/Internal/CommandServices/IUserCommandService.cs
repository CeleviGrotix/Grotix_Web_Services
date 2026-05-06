// Profiles/Application/Internal/CommandServices/IUserCommandService.cs
using GrotixBackend.Profiles.Domain.Model.Aggregates;
using GrotixBackend.Profiles.Domain.Model.Commands;

namespace GrotixBackend.Profiles.Application.Internal.CommandServices;

public interface IUserCommandService
{
    Task<User> Handle(CreateUserCommand command);
    Task<User> Handle(UpdateUserProfileCommand command);
    Task<User> Handle(AssignUserRoleCommand command);
}