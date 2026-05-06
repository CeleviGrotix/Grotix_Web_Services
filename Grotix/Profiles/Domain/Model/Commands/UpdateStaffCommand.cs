using GrotixBackend.Profiles.Domain.Model.Enums;

namespace GrotixBackend.Profiles.Domain.Model.Commands;

public record UpdateStaffCommand(int StaffId, TechnicalRole? TechnicalRole, bool? IsActive);
