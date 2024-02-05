namespace SFA.DAS.AAN.Hub.Data.Entities;
public class Employer
{
    public Guid MemberId { get; set; }
    public virtual Member Member { get; set; } = null!;
}
