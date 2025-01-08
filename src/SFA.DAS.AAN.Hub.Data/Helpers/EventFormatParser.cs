using SFA.DAS.AAN.Hub.Data.Dto;
using SFA.DAS.AAN.Hub.Data.Entities;

namespace SFA.DAS.AAN.Hub.Data.Helpers;

public static class EventFormatParser
{
    public static List<EventFormat> GetEventFormats(EventNotificationSettings settings)
    {

        var eventTypes = settings.EventTypes;

        if (eventTypes == null) throw new ArgumentNullException(nameof(eventTypes));

        return eventTypes
            .Select(x => ParseEventFormat(x.EventType))
            .Where(format => format.HasValue)
            .Cast<EventFormat>()
            .ToList();
    }

    private static EventFormat? ParseEventFormat(string eventType)
    {
        if (Enum.TryParse(eventType, ignoreCase: true, out EventFormat format))
        {
            return format;
        }
        return null;
    }
}
