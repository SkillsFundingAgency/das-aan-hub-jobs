namespace SFA.DAS.AAN.Hub.Data.Entities;

public class MemberNotificationEventFormat
{
    public long Id { get; set; }
    public Guid MemberId { get; set; }
    public Member Member { get; set; }
    public string EventFormat { get; set; } = string.Empty;
    public bool ReceiveNotifications { get; set; }
}