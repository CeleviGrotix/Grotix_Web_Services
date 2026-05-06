using MediatR;
using GrotixBackend.Profiles.Application.Internal.OutboundServices;
using GrotixBackend.Profiles.Domain.Model.Notifications;

namespace GrotixBackend.Profiles.Application.Internal.EventHandlers;

/// <summary>Informe Profile: acciones posteriores al registro (correo bienvenida).</summary>
public sealed class UserRegisteredNotificationHandler(INotificationServiceAdapter notifications)
    : INotificationHandler<UserRegisteredNotification>
{
    public Task Handle(UserRegisteredNotification notification, CancellationToken cancellationToken) =>
        notifications.SendWelcomeEmailAsync(notification.Email, cancellationToken);
}
