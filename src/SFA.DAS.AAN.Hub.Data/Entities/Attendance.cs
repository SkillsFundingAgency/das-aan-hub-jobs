﻿namespace SFA.DAS.AAN.Hub.Data.Entities;
public class Attendance
{
    public Guid Id { get; set; }
    public Guid MemberId { get; set; }
    public bool IsAttending { get; set; }
    public Guid CalendarEventId { get; set; }
}
