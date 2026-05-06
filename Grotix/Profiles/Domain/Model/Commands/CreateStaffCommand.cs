using GrotixBackend.Profiles.Domain.Model.Enums;

namespace GrotixBackend.Profiles.Domain.Model.Commands;

public record CreateStaffCommand(int UserId, TechnicalRole TechnicalRole, DateTime? LastSystemAccess);
