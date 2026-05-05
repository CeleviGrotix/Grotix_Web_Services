using System;

namespace GrotixBackend.IAM.Application.Resources
{
    public record LoginResponse(int IdentityId, int UserId, string Email, bool Success, string Message, string Token = null);
}