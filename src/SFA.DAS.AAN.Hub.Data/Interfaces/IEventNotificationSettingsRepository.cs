using SFA.DAS.AAN.Hub.Data.Dto;
using SFA.DAS.AAN.Hub.Data.Entities;

namespace SFA.DAS.AAN.Hub.Data.Interfaces
{
    public interface IEventNotificationSettingsRepository
    {
        Task<List<EventNotificationSettings>> GetEventNotificationSettingsAsync(CancellationToken cancellationToken, UserType? userType = UserType.Employer);
    }
}