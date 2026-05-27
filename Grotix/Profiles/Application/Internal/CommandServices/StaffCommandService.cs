using GrotixBackend.Profiles.Domain.Model.Aggregates;
using GrotixBackend.Profiles.Domain.Model.Commands;
using GrotixBackend.Profiles.Domain.Model.ValueObjects;
using GrotixBackend.Profiles.Domain.Repositories;

namespace GrotixBackend.Profiles.Application.Internal.CommandServices;

public class StaffCommandService(
    IStaffRepository staffRepository,
    IUserRepository userRepository,
    IProfilesUnitOfWork unitOfWork
) : IStaffCommandService
{
    public async Task<Staff> Handle(CreateStaffCommand command)
    {
        var user = await userRepository.GetByIdAsync(command.UserId)
                   ?? throw new KeyNotFoundException($"Usuario {command.UserId} no encontrado.");

        if (await staffRepository.GetByUserIdAsync(command.UserId) != null)
            throw new InvalidOperationException("Este usuario ya tiene una ficha de staff.");

        var staff = new Staff(command.UserId, command.TechnicalRole, command.LastSystemAccess);
        await staffRepository.AddAsync(staff);

        user.AssignRole((int)RoleType.staff);

        await unitOfWork.CompleteAsync();
        return staff;
    }

    public async Task<Staff> Handle(UpdateStaffCommand command)
    {
        var staff = await staffRepository.GetByIdAsync(command.StaffId)
                    ?? throw new KeyNotFoundException($"Staff {command.StaffId} no encontrado.");

        if (command.TechnicalRole.HasValue)
            staff.UpdateTechnicalRole(command.TechnicalRole.Value);
        if (command.LastSystemAccess.HasValue)
            staff.UpdateLastSystemAccess(command.LastSystemAccess.Value);
        if (command.IsActive.HasValue)
            staff.SetActive(command.IsActive.Value);

        await unitOfWork.CompleteAsync();
        return staff;
    }
}
