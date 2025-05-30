﻿namespace SFA.DAS.AAN.Hub.Data.Entities;

public class MemberNotificationLocation
{
    public long Id { get; set; }
    public Guid MemberId { get; set; }
    public Member Member { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Radius { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}