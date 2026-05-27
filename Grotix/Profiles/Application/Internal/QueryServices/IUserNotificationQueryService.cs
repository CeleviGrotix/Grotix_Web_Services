using GrotixBackend.Profiles.Domain.Model.Aggregates;

namespace GrotixBackend.Profiles.Application.Internal.QueryServices;

public interface IUserNotificationQueryService
{
    Task<IReadOnlyList<UserNotification>> ListByUserAsync(int userId, bool unreadOnly, int limit);
    Task<int> CountUnreadAsync(int userId);
}

