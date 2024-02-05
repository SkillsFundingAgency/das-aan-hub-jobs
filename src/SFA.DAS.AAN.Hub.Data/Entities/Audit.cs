namespace SFA.DAS.AAN.Hub.Data.Entities;
public class Audit
{
    public long Id { get; set; }
    public Guid ActionedBy { get; set; }
    public string Resource { get; set; } = null!;
    public Member Member { get; set; } = null!;
}
