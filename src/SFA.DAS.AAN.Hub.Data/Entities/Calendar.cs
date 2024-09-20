namespace SFA.DAS.AAN.Hub.Data.Entities;
public class Calendar
{
    public int Id { get; set; }
    public string CalendarName { get; set; } = null!;
    public DateTime EffectiveFromDate { get; set; }
    public DateTime EffectiveToDate { get; set; }
    public int Ordering { get; set; }
}
