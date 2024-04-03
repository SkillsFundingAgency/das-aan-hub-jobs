using SFA.DAS.AAN.Hub.Data.Entities;

namespace SFA.DAS.AAN.Hub.Data.Interfaces
{
    public interface INotificationsRepository
    {
        Task<List<Notification>> GetPendingNotifications(int batchSize);
    }
}
