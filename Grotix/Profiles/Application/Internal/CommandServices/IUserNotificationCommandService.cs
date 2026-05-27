using GrotixBackend.Profiles.Domain.Model.Aggregates;

namespace GrotixBackend.Profiles.Application.Internal.CommandServices;

public interface IUserNotificationCommandService
{
    Task<UserNotification> CreateAsync(
        int userId,
        string title,
        string message,
        string? type,
        CancellationToken cancellationToken = default);

    Task<bool> MarkAsReadAsync(
        int userId,
        int notificationId,
        CancellationToken cancellationToken = default);

    Task<int> MarkAllAsReadAsync(
        int userId,
        CancellationToken cancellationToken = default);
}

