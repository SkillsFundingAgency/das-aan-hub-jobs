namespace SFA.DAS.AAN.Hub.Data.Dto;

public class EventListingDTO
{
    public int TotalCount { get; set; }
    public int OnlineCount { get; set; }
    public int HybridCount { get; set; }
    public int InPersonCount { get; set; }
    public IEnumerable<CalendarEventSummary> CalendarEvents { get; set; } = [];
    public string? Location { get; set; } = "";
    public int? Radius { get; set; }
}