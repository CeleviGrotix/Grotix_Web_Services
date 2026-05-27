using GrotixBackend.Profiles.Domain.Model.Aggregates;

namespace GrotixBackend.Profiles.Domain.Repositories;

public interface IUserNotificationRepository
{
    Task<UserNotification?> GetByIdAsync(int id);
    Task<IReadOnlyList<UserNotification>> ListByUserAsync(int userId, bool unreadOnly, int limit);
    Task<int> CountUnreadAsync(int userId);
    Task AddAsync(UserNotification notification);
}

