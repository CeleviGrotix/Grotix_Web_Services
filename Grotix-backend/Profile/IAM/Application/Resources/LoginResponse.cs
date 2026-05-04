using System;

namespace GrotixBackend.IAM.Application.Resources
{
    public record LoginResponse(int IdentityId, int UserId, string Username, bool Success, string Message, string Token = null);
}