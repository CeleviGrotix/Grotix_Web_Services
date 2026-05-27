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
    IAssociationRepository associationRepository,
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
            command.AssociationId,
            profilePicture: null,
            preferences: null,
            isActive: command.IsActive);

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

    public async Task<User> Handle(AdminPatchUserCommand command)
    {
        var user = await userRepository.GetByIdAsync(command.UserId)
                   ?? throw new KeyNotFoundException($"Usuario {command.UserId} no encontrado.");

        var hasProfileChange = command.Name is not null || command.TaxId is not null ||
                               command.Phone is not null || command.ProfilePicture is not null;
        if (hasProfileChange)
        {
            user.UpdateProfile(
                command.Name is not null ? command.Name : user.Name,
                command.TaxId is not null ? command.TaxId : user.TaxId,
                command.Phone is not null ? command.Phone : user.Phone,
                command.ProfilePicture is not null ? command.ProfilePicture : user.ProfilePicture);
        }

        if (command.IsActive.HasValue)
            user.SetActive(command.IsActive.Value);

        if (command.RoleId.HasValue)
        {
            if (!await roleRepository.ExistsAsync(command.RoleId.Value))
                throw new ArgumentException($"El rol {command.RoleId.Value} no existe.");
            user.AssignRole(command.RoleId.Value);
        }

        if (command.AssociationId.HasValue)
        {
            if (!await associationRepository.ExistsAsync(command.AssociationId.Value))
                throw new ArgumentException($"La asociación {command.AssociationId.Value} no existe.");
            user.AssignAssociation(command.AssociationId.Value);
        }

        await unitOfWork.CompleteAsync();
        return user;
    }
}