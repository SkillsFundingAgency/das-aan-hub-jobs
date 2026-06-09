using SFA.DAS.AAN.Hub.Data.Entities;

namespace SFA.DAS.AAN.Hub.Data.Dto;

public class CalendarEventSummary
{
    public Guid CalendarEventId { get; set; }

    public string CalendarName { get; set; } = string.Empty;

    public EventFormat EventFormat { get; set; }

    public DateTime Start { get; set; }

    public DateTime End { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public string Postcode { get; set; } = string.Empty;

    public double? Longitude { get; set; }

    public double? Latitude { get; set; }

    public double? Distance { get; set; }

    public bool IsAttending { get; set; }
}