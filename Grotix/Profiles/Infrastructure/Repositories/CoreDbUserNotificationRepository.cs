using GrotixBackend.Profiles.Domain.Model.Aggregates;
using GrotixBackend.Profiles.Domain.Repositories;
using GrotixBackend.Shared.Infrastructure.Persistence.EFC.Configuration;
using Microsoft.EntityFrameworkCore;

namespace GrotixBackend.Profiles.Infrastructure.Repositories;

public class CoreDbUserNotificationRepository(ProfilesDbContext context) : IUserNotificationRepository
{
    public Task<UserNotification?> GetByIdAsync(int id) =>
        context.Set<UserNotification>().FirstOrDefaultAsync(n => n.Id == id);

    public async Task<IReadOnlyList<UserNotification>> ListByUserAsync(int userId, bool unreadOnly, int limit)
    {
        var safeLimit = limit <= 0 ? 50 : Math.Min(limit, 200);

        var query = context.Set<UserNotification>()
            .AsNoTracking()
            .Where(n => n.UserId == userId);

        if (unreadOnly)
            query = query.Where(n => !n.IsRead);

        return await query
            .OrderByDescending(n => n.CreatedAt)
            .Take(safeLimit)
            .ToListAsync();
    }

    public Task<int> CountUnreadAsync(int userId) =>
        context.Set<UserNotification>()
            .AsNoTracking()
            .CountAsync(n => n.UserId == userId && !n.IsRead);

    public async Task AddAsync(UserNotification notification) =>
        await context.Set<UserNotification>().AddAsync(notification);
}

