using GrotixBackend.Profiles.Application.Internal.OutboundServices;

namespace GrotixBackend.Profiles.Infrastructure.Adapters;

public sealed class NoOpNotificationServiceAdapter : INotificationServiceAdapter
{
    public Task SendWelcomeEmailAsync(string email, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}
