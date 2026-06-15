namespace SFA.DAS.AAN.Hub.Data.Entities;

public class MemberNotificationEventFormat
{
    public long Id { get; set; }
    public Guid MemberId { get; set; }
    public Member Member { get; set; } = null!;
    public string EventFormat { get; set; } = null!;
    public bool ReceiveNotifications { get; set; }
}