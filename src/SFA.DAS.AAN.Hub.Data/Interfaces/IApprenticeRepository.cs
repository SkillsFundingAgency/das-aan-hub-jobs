using SFA.DAS.AAN.Hub.Data.Entities;

namespace SFA.DAS.AAN.Hub.Data.Interfaces
{
    public interface IApprenticeRepository
    {
        Task<List<Apprentice>> GetApprentices(CancellationToken cancellationToken);

        Task<List<Apprentice>> GetApprentices(CancellationToken cancellationToken, Guid[] ids);
    }
}
