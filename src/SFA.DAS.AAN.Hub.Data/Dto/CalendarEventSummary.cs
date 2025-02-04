using SFA.DAS.AAN.Hub.Data.Entities;

namespace SFA.DAS.AAN.Hub.Data.Dto;

public class CalendarEventSummary
{
    public Guid CalendarEventId { get; set; }

    public string CalendarName { get; set; }

    public EventFormat EventFormat { get; set; }

    public DateTime Start { get; set; }

    public DateTime End { get; set; }

    public string Title { get; set; }

    public string Summary { get; set; }

    public string Location { get; set; }

    public string Postcode { get; set; }

    public double? Longitude { get; set; }

    public double? Latitude { get; set; }

    public double? Distance { get; set; }

    public bool IsAttending { get; set; }
}