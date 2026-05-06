namespace GrotixBackend.Profiles.Interfaces.REST.Resources;

public record StaffResource(int Id, int UserId, string TechnicalRole, bool IsActive);
