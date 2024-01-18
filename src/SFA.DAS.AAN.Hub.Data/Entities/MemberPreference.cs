namespace SFA.DAS.AAN.Hub.Data.Entities;
public class MemberPreference
{
    public long Id { get; set; }
    public Guid MemberId { get; set; }
    public Member Member { get; set; } = null!;
}
