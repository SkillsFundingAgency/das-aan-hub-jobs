namespace SFA.DAS.AAN.Hub.Data.Dto;

public class GetCalendarEventsQueryResult
{
    public int TotalCount { get; set; }
    public IEnumerable<CalendarEventSummary> CalendarEvents { get; set; } = [];
}