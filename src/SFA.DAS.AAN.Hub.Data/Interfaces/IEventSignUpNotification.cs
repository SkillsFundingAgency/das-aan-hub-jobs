using SFA.DAS.AAN.Hub.Data.Dto;

namespace SFA.DAS.AAN.Hub.Data.Interfaces
{
    public interface IEventSignUpNotificationRepository
    {
        Task<List<EventSignUpNotification>> GetEventSignUpNotification();
    }
}
