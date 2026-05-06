namespace GrotixBackend.Profiles.Domain.Model.Commands;

public record AssignUserRoleCommand(int UserId, int RoleId);
