using Microsoft.EntityFrameworkCore;
using SFA.DAS.AAN.Hub.Data.Dto;
using SFA.DAS.AAN.Hub.Data.Interfaces;

namespace SFA.DAS.AAN.Hub.Data.Repositories;

public class EventNotificationSettingsRepository : IEventNotificationSettingsRepository
{
    private readonly IAanDataContext _context;

    public EventNotificationSettingsRepository(IAanDataContext context)
    {
        _context = context;
    }

    public async Task<List<EventNotificationSettings>> GetEventNotificationSettings()
    {
        // criteria: all members that are of type Employer and have ReceiveNotifications set to YES
        return await _context.Members
            .AsNoTracking()
            .Where(m => m.UserType == Entities.UserType.Employer && m.ReceiveNotifications == true)
            .Select(a => new EventNotificationSettings
            {
                MemberDetails = new MemberDetails 
                {
                    Id = a.Id,
                    FirstName = a.FirstName,
                    Email = a.Email
                },
                ReceiveNotifications = a.ReceiveNotifications
            })
            .ToListAsync();
    }

}
