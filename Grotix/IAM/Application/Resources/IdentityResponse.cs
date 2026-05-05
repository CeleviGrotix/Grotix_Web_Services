using System;

namespace GrotixBackend.IAM.Application.Resources
{
    public record IdentityResponse(int Id, int UserId, string Email, DateTime CreatedAt, DateTime? LastLoginAt, bool IsActive);
}