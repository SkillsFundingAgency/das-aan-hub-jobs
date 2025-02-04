namespace SFA.DAS.AAN.Hub.Data.Dto;

public class EventListingDTO
{
    public int TotalCount { get; set; }
    public IEnumerable<CalendarEventSummary> CalendarEvents { get; set; } = [];
    public string? Location { get; set; } = "";
    public int? Radius { get; set; }
}