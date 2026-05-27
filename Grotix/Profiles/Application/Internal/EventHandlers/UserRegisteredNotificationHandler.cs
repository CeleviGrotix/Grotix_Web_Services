using MediatR;
using GrotixBackend.Contracts.Auth.Notifications;
using GrotixBackend.Profiles.Application.Internal.OutboundServices;

namespace GrotixBackend.Profiles.Application.Internal.EventHandlers;

/// <summary>Informe Profile: acciones posteriores al registro (correo bienvenida).</summary>
public sealed class UserRegisteredNotificationHandler(INotificationServiceAdapter notifications)
    : INotificationHandler<UserRegisteredNotification>
{
    public Task Handle(UserRegisteredNotification notification, CancellationToken cancellationToken) =>
        notifications.SendWelcomeEmailAsync(notification.Email, cancellationToken);
}
