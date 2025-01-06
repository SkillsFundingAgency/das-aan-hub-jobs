using Microsoft.EntityFrameworkCore;
using SFA.DAS.AAN.Hub.Data.Dto;
using SFA.DAS.AAN.Hub.Data.Interfaces;
using static SFA.DAS.AAN.Hub.Data.Dto.EventNotificationSettings;

namespace SFA.DAS.AAN.Hub.Data.Repositories;

public class EventNotificationSettingsRepository : IEventNotificationSettingsRepository
{
    private readonly IAanDataContext _context;

    public EventNotificationSettingsRepository(IAanDataContext context)
    {
        _context = context;
    }

    public async Task<List<EventNotificationSettings>> GetEventNotificationSettingsAsync(CancellationToken cancellationToken)
    {
        return await _context.Members
            .AsNoTracking()
            .Where(m => m.UserType == Entities.UserType.Employer && m.ReceiveNotifications == true)
            .Include(m => m.MemberNotificationEventFormats)
            .Include(m => m.MemberNotificationLocations)
            .AsSplitQuery()
            .Select(a => new EventNotificationSettings
            {
                MemberDetails = new MemberDetails 
                {
                    Id = a.Id,
                    FirstName = a.FirstName,
                    Email = a.Email
                },
                ReceiveNotifications = a.ReceiveNotifications,
                EventTypes = a.MemberNotificationEventFormats.Select(e => new NotificationEventType
                {
                    EventType = e.EventFormat,
                    ReceiveNotifications = a.ReceiveNotifications
                }).ToList(),
                Locations = a.MemberNotificationLocations.Select(loc => new Location
                {
                    Name = loc.Name,
                    Radius = loc.Radius,
                    Latitude = loc.Latitude,
                    Longitude = loc.Longitude
                }).ToList()
            })
            .ToListAsync(cancellationToken);
    }
}
