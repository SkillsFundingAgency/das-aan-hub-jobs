using SFA.DAS.AAN.Hub.Data.Dto;

namespace SFA.DAS.AAN.Hub.Data.Interfaces
{
    public interface IEventNotificationSettingsRepository
    {
        Task<List<EventNotificationSettings>> GetEventNotificationSettingsAsync(CancellationToken cancellationToken);
    }
}