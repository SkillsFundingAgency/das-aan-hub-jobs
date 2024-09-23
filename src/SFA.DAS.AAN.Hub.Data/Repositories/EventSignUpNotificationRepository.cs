using Microsoft.EntityFrameworkCore;
using SFA.DAS.AAN.Hub.Data.Dto;
using SFA.DAS.AAN.Hub.Data.Interfaces;

namespace SFA.DAS.AAN.Hub.Data.Repositories;

public class EventSignUpNotificationRepository : IEventSignUpNotificationRepository
{
    private readonly IAanDataContext _context;

    public EventSignUpNotificationRepository(IAanDataContext context)
    {
        _context = context;
    }

    public async Task<List<EventSignUpNotification>> GetEventSignUpNotification()
    {
        var last24Hours = DateTime.UtcNow.AddHours(-96);

        return await _context.Attendances
            .AsNoTracking()
            .Where(a => a.AddedDate >= last24Hours && a.CalendarEvent.Member.ReceiveNotifications)
            .Select(a => new EventSignUpNotification
            {
                CalendarEventId = a.CalendarEvent.Id,
                CalendarName = a.CalendarEvent.Calender.CalendarName,
                EventFormat = a.CalendarEvent.EventFormat,
                EventTitle = a.CalendarEvent.Title,
                StartDate = a.CalendarEvent.StartDate,
                EndDate = a.CalendarEvent.EndDate,
                FirstName = a.CalendarEvent.Member.FirstName,
                LastName = a.CalendarEvent.Member.LastName,
                AdminMemberId = a.CalendarEvent.Member.Id,
                NewAmbassadorsCount = _context.Attendances
                    .Count(att => att.CalendarEventId == a.CalendarEvent.Id && att.AddedDate >= last24Hours),
                TotalAmbassadorsCount = _context.Attendances
                    .Count(att => att.CalendarEventId == a.CalendarEvent.Id)
            })
            .Distinct()
            .ToListAsync();
    }

}
