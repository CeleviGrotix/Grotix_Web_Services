// Profiles/Application/Internal/CommandServices/UserCommandService.cs
using GrotixBackend.Profiles.Domain.Model.Aggregates;
using GrotixBackend.Profiles.Domain.Model.Commands;
using GrotixBackend.Profiles.Domain.Repositories;
using GrotixBackend.Shared.Domain.Repositories;

namespace GrotixBackend.Profiles.Application.Internal.CommandServices;

public class UserCommandService(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork
) : IUserCommandService
{
    public async Task<User> Handle(CreateUserCommand command)
    {
        var user = new User(
            command.IdentityId,
            command.Email,
            command.RoleId,
            command.Name,
            command.TaxId,
            command.Phone
        );

        await userRepository.AddAsync(user);
        await unitOfWork.CompleteAsync();
        return user;
    }
}