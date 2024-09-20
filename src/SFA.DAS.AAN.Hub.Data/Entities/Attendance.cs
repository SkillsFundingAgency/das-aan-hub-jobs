namespace SFA.DAS.AAN.Hub.Data.Entities;
public class Attendance
{
    public Guid Id { get; set; }
    public Guid MemberId { get; set; }
    public bool IsAttending { get; set; }
    public DateTime AddedDate { get; set; }
    public Guid CalendarEventId { get; set; }
    public virtual CalendarEvent CalendarEvent { get; set; } = null!;
}
