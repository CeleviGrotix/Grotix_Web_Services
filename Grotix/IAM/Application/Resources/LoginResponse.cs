namespace GrotixBackend.IAM.Application.Resources;

public record LoginResponse(
    int IdentityId,
    string Email,
    bool Success,
    string Message,
    string? Token = null
);