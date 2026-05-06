using GrotixBackend.Profiles.Domain.Model.Aggregates;
using GrotixBackend.Profiles.Domain.Model.Commands;

namespace GrotixBackend.Profiles.Application.Internal.CommandServices;

public interface IStaffCommandService
{
    Task<Staff> Handle(CreateStaffCommand command);
    Task<Staff> Handle(UpdateStaffCommand command);
}
