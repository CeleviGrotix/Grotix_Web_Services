using GrotixBackend.Profiles.Application.Internal.QueryServices;
using GrotixBackend.Profiles.Domain.Model.Aggregates;
using GrotixBackend.Profiles.Domain.Repositories;

namespace GrotixBackend.Profiles.Application.Internal.CommandServices;

public class UserNotificationQueryService(IUserNotificationRepository notificationRepository)
    : IUserNotificationQueryService
{
    public Task<IReadOnlyList<UserNotification>> ListByUserAsync(int userId, bool unreadOnly, int limit) =>
        notificationRepository.ListByUserAsync(userId, unreadOnly, limit);

    public Task<int> CountUnreadAsync(int userId) =>
        notificationRepository.CountUnreadAsync(userId);
}

