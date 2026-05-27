using GrotixBackend.Profiles.Domain.Model.Aggregates;
using GrotixBackend.Profiles.Domain.Repositories;

namespace GrotixBackend.Profiles.Application.Internal.CommandServices;

public class UserNotificationCommandService(
    IUserRepository userRepository,
    IUserNotificationRepository notificationRepository,
    IProfilesUnitOfWork unitOfWork) : IUserNotificationCommandService
{
    public async Task<UserNotification> CreateAsync(
        int userId,
        string title,
        string message,
        string? type,
        CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new KeyNotFoundException($"Usuario {userId} no encontrado.");

        var notification = new UserNotification(userId, title, message, type);
        await notificationRepository.AddAsync(notification);
        await unitOfWork.CompleteAsync();
        return notification;
    }

    public async Task<bool> MarkAsReadAsync(
        int userId,
        int notificationId,
        CancellationToken cancellationToken = default)
    {
        var notification = await notificationRepository.GetByIdAsync(notificationId);
        if (notification == null || notification.UserId != userId)
            return false;

        notification.MarkAsRead();
        await unitOfWork.CompleteAsync();
        return true;
    }

    public async Task<int> MarkAllAsReadAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        var unread = await notificationRepository.ListByUserAsync(userId, unreadOnly: true, limit: 1000);
        if (unread.Count == 0)
            return 0;

        foreach (var notification in unread)
        {
            // AsNoTracking no aplica aquí porque luego los volvemos a cargar por id para asegurar tracking.
            var tracked = await notificationRepository.GetByIdAsync(notification.Id);
            tracked?.MarkAsRead();
        }

        await unitOfWork.CompleteAsync();
        return unread.Count;
    }
}

