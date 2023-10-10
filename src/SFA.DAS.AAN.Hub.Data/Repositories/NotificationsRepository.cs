using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.AAN.Hub.Data.Entities;

namespace SFA.DAS.AAN.Hub.Data.Repositories;

public interface INotificationsRepository
{
    Task<List<Notification>> GetPendingNotifications(int batchSize);
}

[ExcludeFromCodeCoverage]
internal class NotificationsRepository : INotificationsRepository
{
    private readonly IAanDataContext _context;

    public NotificationsRepository(IAanDataContext context)
    {
        _context = context;
    }

    public async Task<List<Notification>> GetPendingNotifications(int batchSize)
    {
        var query = _context
       .Notifications
       .Where(n => n.SentTime == null && (n.SendAfterTime == null || n.SendAfterTime <= DateTime.UtcNow))
       .OrderByDescending(n => n.IsSystem)
       .Take(batchSize);
        return await query.ToListAsync();
    }
}
