using MediatR;

namespace GrotixBackend.Contracts.Auth.Notifications;

public record UserRegisteredNotification(int IdentityId, string Email) : INotification;
