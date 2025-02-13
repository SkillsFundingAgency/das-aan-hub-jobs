using SFA.DAS.AAN.Hub.Data.Dto;
using SFA.DAS.AAN.Hub.Data.Entities;

namespace SFA.DAS.AAN.Hub.Data.Interfaces;

public interface IApprenticeEventQueryService
{
    Task<List<EventListingDTO>> GetEventListings(EventNotificationSettings notificationSettings, List<EventFormat> eventFormats, CancellationToken cancellationToken);
}