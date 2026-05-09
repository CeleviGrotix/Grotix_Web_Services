// Profiles/Application/Internal/CommandServices/UserCommandService.cs
using GrotixBackend.Profiles.Domain.Model.Aggregates;
using GrotixBackend.Profiles.Domain.Model.Commands;
using GrotixBackend.Profiles.Domain.Model.ValueObjects;
using GrotixBackend.Profiles.Domain.Repositories;
using GrotixBackend.Shared.Domain.Repositories;

namespace GrotixBackend.Profiles.Application.Internal.CommandServices;

public class UserCommandService(
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    IUnitOfWork unitOfWork
) : IUserCommandService
{
    public async Task<User> Handle(CreateUserCommand command)
    {
        if (!await roleRepository.ExistsAsync(command.RoleId))
            throw new ArgumentException($"El rol {command.RoleId} no existe.");

        var email = UserEmail.Create(command.Email);
        var user = new User(
            command.IdentityId,
            email,
            command.RoleId,
            command.Name,
            command.TaxId,
            command.Phone,
            command.AssociationId
        );

        await userRepository.AddAsync(user);
        await unitOfWork.CompleteAsync();
        return user;
    }

    public async Task<User> Handle(UpdateUserProfileCommand command)
    {
        var user = await userRepository.GetByIdAsync(command.UserId)
                   ?? throw new KeyNotFoundException($"Usuario {command.UserId} no encontrado.");
        user.UpdateProfile(command.Name, command.TaxId, command.Phone, command.ProfilePicture);
        await unitOfWork.CompleteAsync();
        return user;
    }

    public async Task<User> Handle(AssignUserRoleCommand command)
    {
        if (!await roleRepository.ExistsAsync(command.RoleId))
            throw new ArgumentException($"El rol {command.RoleId} no existe.");
        var user = await userRepository.GetByIdAsync(command.UserId)
                   ?? throw new KeyNotFoundException($"Usuario {command.UserId} no encontrado.");
        user.AssignRole(command.RoleId);
        await unitOfWork.CompleteAsync();
        return user;
    }
}