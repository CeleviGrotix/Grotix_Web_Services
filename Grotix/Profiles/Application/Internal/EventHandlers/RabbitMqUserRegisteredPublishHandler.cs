using System.Text;
using System.Text.Json;
using GrotixBackend.BuildingBlocks.RabbitMq;
using GrotixBackend.Contracts.Integration.Users;
using GrotixBackend.Profiles.Domain.Model.Notifications;
using MediatR;
using Microsoft.Extensions.Options;

namespace GrotixBackend.Profiles.Application.Internal.EventHandlers;

/// <summary>Publica <see cref="UserRegisteredNotification"/> a RabbitMQ (integración asíncrona).</summary>
public sealed class RabbitMqUserRegisteredPublishHandler(
    IRabbitMqPublisher publisher,
    IOptions<RabbitMqOptions> options) : INotificationHandler<UserRegisteredNotification>
{
    public Task Handle(UserRegisteredNotification notification, CancellationToken cancellationToken)
    {
        if (!options.Value.Enabled)
            return Task.CompletedTask;

        var payload = new UserRegisteredIntegrationEvent(notification.IdentityId, notification.Email);
        var json = JsonSerializer.Serialize(payload);
        publisher.Publish(options.Value.UserRegisteredRoutingKey, Encoding.UTF8.GetBytes(json));
        return Task.CompletedTask;
    }
}
