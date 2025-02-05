using SFA.DAS.AAN.Hub.Data.Dto;
using SFA.DAS.AAN.Hub.Data.Entities;
using System.ComponentModel;

namespace SFA.DAS.AAN.Hub.Data.Helpers;

public static class EventFormatParser
{
    public static List<EventFormat> GetEventFormats(EventNotificationSettings settings)
    {
        var eventTypes = settings.EventTypes;

        if (eventTypes == null) throw new ArgumentNullException(nameof(eventTypes));

        var allFormats = eventTypes
            .Where(t => t.ReceiveNotifications == true)
            .Select(x => ParseEventFormat(x.EventType))
            .Where(format => format.HasValue)
            .Cast<EventFormat>()
            .ToList();

        if (allFormats.Contains(EventFormat.All))
        {
            return Enum.GetValues(typeof(EventFormat))
                .Cast<EventFormat>()
                .Where(format => format != EventFormat.All)
                .ToList();
        }

        return allFormats;
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
