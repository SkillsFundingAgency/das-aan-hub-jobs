using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.AAN.Hub.Data.Entities;
using SFA.DAS.AAN.Hub.Data.Interfaces;

namespace SFA.DAS.AAN.Hub.Data.Repositories;

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
        var query = _context.Notifications
           .Include(n => n.Member)
           .Where(n => n.SentTime == null && (n.SendAfterTime == null || n.SendAfterTime <= DateTime.UtcNow))
           .OrderBy(n => n.IsSystem)
           .ThenByDescending(n => n.CreatedDate)
           .ThenBy(n => n.SendAfterTime)
           .Take(batchSize);

        return await query.ToListAsync();
    }
}
